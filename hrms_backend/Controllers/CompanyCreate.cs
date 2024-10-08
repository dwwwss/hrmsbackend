using System;
using hrms_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyCreate : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;
        private readonly IOptions<EmailConfiguration> _emailConfigOptions;

        public CompanyCreate(HrmsDbContext dbContext, IOptions<EmailConfiguration> emailConfigOptions)
        {
            _dbContext = dbContext;
            _emailConfigOptions = emailConfigOptions;
        }

        public class CompanyEmployeeRequest
        {
            public Company Company { get; set; }
            public Employee Employee { get; set; }
        }

        [HttpPost("create")]
        public IActionResult CreateCompanyWithAdmin([FromBody] CompanyEmployeeRequest request)
        {
            try
            {
                // Validate request
                if (request == null ||
                    request.Company == null ||
                    string.IsNullOrEmpty(request.Company.CompanyName) ||
                    request.Employee == null ||
                    string.IsNullOrEmpty(request.Employee.Email) ||
                    string.IsNullOrEmpty(request.Employee.Password))
                {
                    return BadRequest("Invalid request. Company name, email, and password are required.");
                }

                if (_dbContext.Companies.Any(c => c.CompanyName == request.Company.CompanyName))
                {
                    return BadRequest("Company with the same name already exists.");
                }

                if (_dbContext.Employees.Any(e => e.Email == request.Employee.Email))
                {
                    return BadRequest("Employee with the same email already exists.");
                }

                // Get the maximum CompanyId from the Companies table
                int maxCompanyId = _dbContext.Companies.Max(c => (int?)c.CompanyId) ?? 0;

                // Increment the CompanyId
                int companyId = maxCompanyId + 1;

                // Set the CompanyId
                request.Company.CompanyId = companyId;

                // Set the CompanyGuid before adding to the context
                request.Company.CompanyGuid = Guid.NewGuid();

                // Add the company to the context
                _dbContext.Companies.Add(request.Company);
                _dbContext.SaveChanges();

                // Get the maximum RoleId from the Roles table
                int maxRoleId = _dbContext.Roles.Max(r => (int?)r.RoleId) ?? 0;

                // Increment the RoleId
                int roleId = maxRoleId + 1;

                // Create a new role named "Superadmin" for the company
                Role superAdminRole = new Role
                {
                    RoleId = roleId, // Explicitly set the RoleId
                    RoleName = "Superadmin",
                    CompanyId = companyId
                };

                _dbContext.Roles.Add(superAdminRole);
                _dbContext.SaveChanges();

                // Iterate over modules and assign default permission
                var modules = _dbContext.Modules.ToList(); // Assuming you have a Modules table
                var defaultPermissionIds = 1; // Change this to the desired permission ID

                foreach (var module in modules)
                {
                    // Create a new entry in RoleModulePermissionMapping for each module
                    var mapping = new RoleModulePermissionMapping
                    {
                        RoleId = superAdminRole.RoleId,
                        ModuleId = module.ModuleId,
                        PermissionFlag = defaultPermissionIds,
                    };

                    _dbContext.RoleModulePermissionMappings.Add(mapping);
                }

                _dbContext.SaveChanges();

                // Get the maximum EmployeeId from the Employees table
                int maxEmployeeId = _dbContext.Employees.Max(e => (int?)e.EmployeeId) ?? 0;

                // Increment the EmployeeId
                int employeeId = maxEmployeeId + 1;

                // Set the EmployeeId
                request.Employee.EmployeeId = employeeId;

                // Set the FkCompanyId for the employee
                request.Employee.FkCompanyId = companyId;

                // Set the role id for the employee
                request.Employee.fk_role_id = superAdminRole.RoleId;

                // Create a default admin user (employee) for the company
                _dbContext.Employees.Add(request.Employee);
                _dbContext.SaveChanges();

                // Create and add related Address object
                var address = new Address
                {
                    AddressId = _dbContext.Addresses.Max(a => (int?)a.AddressId) + 1 ?? 1,
                    FkEmployeeId = employeeId,
                    // Set other properties as needed
                };
                _dbContext.Addresses.Add(address);

                // Create and add related BankInformation object
                var bankInformation = new BankInformation
                {
                    BankInfoId = _dbContext.BankInformations.Max(bi => (int?)bi.BankInfoId) + 1 ?? 1,
                    FkEmployeeId = employeeId,
                    // Set other properties as needed
                };
                _dbContext.BankInformations.Add(bankInformation);

                // Create and add related EmergencyContact object
                var emergencyContact = new EmergencyContact
                {
                    EmergencyContactId = _dbContext.EmergencyContacts.Max(ec => (int?)ec.EmergencyContactId) + 1 ?? 1,
                    FkEmployeeId = employeeId,
                    // Set other properties as needed
                };
                _dbContext.EmergencyContacts.Add(emergencyContact);

                // Get the maximum SectionPermissionId from the SectionPermissions table
                int maxSectionPermissionId = _dbContext.SectionPermissions.Max(sp => (int?)sp.SectionPermissionId) ?? 0;

                // Increment the SectionPermissionId
                int sectionPermissionId = maxSectionPermissionId + 1;

                // Automatically create SectionPermission entries for the new role
                var sections = _dbContext.Sections.ToList(); // Assuming you have a Sections table
                var defaultPermissionId = 2; // Change this to the desired permission ID

                foreach (var section in sections)
                {
                    var sectionPermission = new SectionPermission
                    {
                        SectionPermissionId = sectionPermissionId++, // Increment the SectionPermissionId
                        RoleId = superAdminRole.RoleId,
                        SectionId = section.SectionId,
                        PermissionFlag = defaultPermissionId,
                        CompanyId = companyId
                    };

                    _dbContext.SectionPermissions.Add(sectionPermission);
                }
                int superAdminRoleId = superAdminRole.RoleId;

                // Create HR role
                int hrRoleId = CreateRole("HR", companyId);

                // Create Employee role
                int employeeRoleId = CreateRole("Employee", companyId);

                // Assign section permissions for HR role
                AssignSectionPermissions(hrRoleId, companyId);

                // Assign section permissions for Employee role
                AssignSectionPermissions(employeeRoleId, companyId);
                // Create two default stages for the company
                var stage1 = new Stage
                {
                    StageName = "Offered",
                    FkCompanyId = companyId
                    // You can set other properties as needed
                };

                var stage2 = new Stage
                {
                    StageName = "Rejection",
                    FkCompanyId = companyId
                    // You can set other properties as needed
                };
                var stage3 = new Stage
                {
                    StageName = "Applied",
                    FkCompanyId = companyId
                    // You can set other properties as needed
                };
                var stage4 = new Stage
                {
                    StageName = "Hired",
                    FkCompanyId = companyId
                    // You can set other properties as needed
                };

                _dbContext.Stages.Add(stage1);
                _dbContext.Stages.Add(stage2);
                _dbContext.Stages.Add(stage3);
                _dbContext.Stages.Add(stage4);
                _dbContext.SaveChanges();

                // Create email templates for the stages
                var emailTemplate1 = new EmailTemplate
                {
                    Subject = "Welcome to Stage 1",
                    Stage = "Offered",
                    FkCompanyId = companyId,
                    Body = "Hi {Fullname},\r\n\r\n\r\n\r\nThank you so much for your interest in the {{job_title}} job at {CompanyName}.\r\n\r\n\r\n\r\nUnfortunately, we have decided to move forward with other candidates for this position, but we would like to thank you for talking to our team and giving us the opportunity to learn about your skills and accomplishments. \r\n\r\n\r\n\r\nWe wish you good luck with your job search and professional future endeavors.\r\n\r\n\r\n\r\nBest regards,\r\n\r\n\r\n\r\n{CompanyName}",
                    EmailTemplate1 = "Offer Letter",
                    LastModified = DateTime.Now,
                    // You can set other properties as needed
                };

                var emailTemplate2 = new EmailTemplate
                {
                    Subject = "Welcome to Stage 2",
                    Stage = "Rejection",
                    FkCompanyId = companyId,
                    Body = "Hi {Fullname},\r\n\r\n\r\n\r\nThank you so much for your interest in the {{job_title}} job at {CompanyName}.\r\n\r\n\r\n\r\nUnfortunately, we have decided to move forward with other candidates for this position, but we would like to thank you for talking to our team and giving us the opportunity to learn about your skills and accomplishments. \r\n\r\n\r\n\r\nWe wish you good luck with your job search and professional future endeavors.\r\n\r\n\r\n\r\nBest regards,\r\n\r\n\r\n\r\n{CompanyName}",
                    EmailTemplate1 = "Reject Letter",
                    LastModified = DateTime.Now,
                    // You can set other properties as needed
                };

                _dbContext.EmailTemplates.Add(emailTemplate1);
                _dbContext.EmailTemplates.Add(emailTemplate2);
                _dbContext.SaveChanges();
                SendActivationEmail(request.Employee.Email, request.Employee.EmployeeId, companyId);
                return Ok("A verification email has been sent to your email address");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating company: {ex.Message}");
            }
        }
        private int CreateRole(string roleName, int companyId)
        {
            int maxRoleId = _dbContext.Roles.Max(r => (int?)r.RoleId) ?? 0;
            int roleId = maxRoleId + 1;

            Role role = new Role
            {
                RoleId = roleId,
                RoleName = roleName,
                CompanyId = companyId
            };

            _dbContext.Roles.Add(role);
            _dbContext.SaveChanges();

            return roleId;
        }
        private void AssignSectionPermissions(int roleId, int companyId)
        {
            int maxSectionPermissionId = _dbContext.SectionPermissions.Max(sp => (int?)sp.SectionPermissionId) ?? 0;
            int sectionPermissionId = maxSectionPermissionId + 1;

            var sections = _dbContext.Sections.ToList();

            foreach (var section in sections)
            {
                var sectionPermission = new SectionPermission
                {
                    SectionPermissionId = sectionPermissionId++,
                    RoleId = roleId,
                    SectionId = section.SectionId,
                    PermissionFlag = 1, // Set the desired permission ID for HR or Employee
                    CompanyId = companyId
                };

                _dbContext.SectionPermissions.Add(sectionPermission);
            }

            _dbContext.SaveChanges();
        }
        private void SendActivationEmail(string toEmail, int employeeId, int companyId)
        {
            // Update the following details based on your SMTP server and email template
            EmailConfiguration emailConfig = _emailConfigOptions.Value;

            // Access configuration values
            string smtpServer = emailConfig.SmtpServer;
            int smtpPort = emailConfig.SmtpPort;
            string smtpUsername = emailConfig.SmtpUsername;
            string smtpPassword = emailConfig.SmtpPassword;
            bool enableSsl = emailConfig.EnableSsl;
            string fromEmail = emailConfig.FromEmail;
            string fromDisplayName = emailConfig.FromDisplayName;

            string subject = "Activate Your Account";

            // Generate a unique activation token
            string activationToken = Guid.NewGuid().ToString();

            // Save the activation token in your database along with employeeId and companyId
            SaveActivationToken(employeeId, companyId, activationToken);

            string activationLink = $"http://10.0.0.168/#/loading?token={activationToken}\r\n";

            // HTML body with a well-designed activation button
            string body = $@"
        <html>
        <head>
            <!-- Add your styles for a visually appealing email here -->
        </head>
        <body>
            <div>
                <p>Dear User,</p>
                <p>Thank you for registering with our HRMS platform. To activate your account, please click the button below:</p>
                <a href='{activationLink}'>Activate Account</a>
                <p>If you have trouble clicking the button, you can also <a href='{activationLink}'>click here</a> or copy and paste the following URL into your browser:</p>
                <p>{activationLink}</p>
                <p>Best regards,<br>HRMS Team</p>
            </div>
        </body>
        </html>
    ";
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(fromEmail, fromDisplayName);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = enableSsl;

                    try
                    {
                        smtpClient.Send(mailMessage);
                        Console.WriteLine($"Activation email sent to {toEmail}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending activation email: {ex.Message}");
                        // Handle the error appropriately (e.g., log it or inform the user)
                    }
                }
            }
        }

        // Save activation token in the database
        private void SaveActivationToken(int employeeId, int companyId, string activationToken)
        {
            // Save the activation token along with employeeId and companyId in your database
            // You can use Entity Framework or any other database access mechanism here
            // For example, if you are using Entity Framework:
            using (var dbContext = new HrmsDbContext())
            {
                var user = dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId && e.FkCompanyId == companyId);
                if (user != null)
                {
                    user.ActivationToken = activationToken;
                    dbContext.SaveChanges();
                }
            }
        }
        [HttpGet("activate")]
        public IActionResult ActivateAccount([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid activation token.");
            }

            using (var dbContext = new HrmsDbContext())
            {
                var user = dbContext.Employees.FirstOrDefault(e => e.ActivationToken == token);

                if (user != null)
                {
                    // Check if the token is not expired (you need to implement token expiration logic)
                    if (IsValidToken(user.ActivationToken))
                    {
                        // Activate the user (e.g., set IsActive to true)
                        user.IsVerified = true;

                        // Reset the activation token to null or mark it as used
                        user.ActivationToken = null;

                        dbContext.SaveChanges();

                        // Redirect to the login page
                        return Ok("Verification Sucessfull");
                    }
                    else
                    {
                        // Token is expired
                        return BadRequest("Activation token has expired.");
                    }
                }
                else
                {
                    // User not found or token is invalid
                    return BadRequest("Invalid activation token.");
                }
            }
        }


        private bool IsValidToken(string token)
        {
            // You need to implement logic to check if the token is still valid
            // For example, check if the token was generated within a certain timeframe
            // and hasn't expired yet.
            // Return true if valid, false otherwise.
            return true; // Replace with your actual token validation logic
        }

    }
}
