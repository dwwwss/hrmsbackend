using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using hrms_backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using System.Net.Mail;
using System.Net;

namespace hrms_backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;

        public EmployeeController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpPost]
        public IActionResult CreateNewEmployee([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest("Invalid data. Employee object is null.");
            }

            try
            {
                // Check if an employee with the same email already exists
                if (_dbContext.Employees.Any(e => e.Email == employee.Email))
                {
                    return BadRequest("An employee with the same email already exists.");
                }

                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
                {
                    // Handle the case where company information is not available in the token
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Check if any of the related tables are empty
                bool isAddressTableEmpty = !_dbContext.Addresses.Any();
                bool isBankInfoTableEmpty = !_dbContext.BankInformations.Any();
                bool isEmergencyContactTableEmpty = !_dbContext.EmergencyContacts.Any();

                // Generate a random password
                string generatedPassword = GenerateRandomPassword();

                // Generate a reset token
                string resetToken = Guid.NewGuid().ToString();

                // Fetch the maximum EmployeeId from the database
                int maxEmployeeId = _dbContext.Employees.Max(e => e.EmployeeId);

                // Increment the EmployeeId by 1 for the new employee
                employee.EmployeeId = maxEmployeeId + 1;

                // Set the created date as the join date
                // ... (set other properties as needed)

                employee.FkCompanyId = companyId;
                // Set the generated password
                employee.Password = generatedPassword;

                // Set IsPasswordGenerated to 1 for the new employee
                employee.IsPasswordGenerated = 1;

                // Set the reset token
                employee.ResetToken = resetToken;

                // Add the employee to the context
                _dbContext.Employees.Add(employee);
                // Create and add related Address object
                var address = new Address
                {
                    AddressId = isAddressTableEmpty ? 1 : _dbContext.Addresses.Max(a => a.AddressId) + 1,
                    FkEmployeeId = employee.EmployeeId,
                    // Set other properties as needed
                };
                _dbContext.Addresses.Add(address);

                // Create and add related BankInformation object
                var bankInformation = new BankInformation
                {
                    BankInfoId = isBankInfoTableEmpty ? 1 : _dbContext.BankInformations.Max(bi => bi.BankInfoId) + 1,
                    FkEmployeeId = employee.EmployeeId,
                    // Set other properties as needed
                };
                _dbContext.BankInformations.Add(bankInformation);

                // Create and add related EmergencyContact object
                var emergencyContact = new EmergencyContact
                {
                    EmergencyContactId = isEmergencyContactTableEmpty ? 1 : _dbContext.EmergencyContacts.Max(ec => ec.EmergencyContactId) + 1,
                    FkEmployeeId = employee.EmployeeId,
                    // Set other properties as needed
                };
                _dbContext.EmergencyContacts.Add(emergencyContact);

                // Save all changes to the database
                _dbContext.SaveChanges();

                // Send email to the new employee with the generated password
                SendEmail(employee, generatedPassword);

                return Ok("Employee created successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception or return an appropriate error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while creating the employee. {ex.Message}");
            }
        }

        private void SendEmail(Employee employee, string generatedPassword)
        {
            var companyNameClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyName");
            string companyName = companyNameClaim?.Value;

            // Ensure that the employee's email is not empty or null
            if (string.IsNullOrEmpty(employee.Email))
            {
                // Log or handle the error appropriately
                // You might want to return or log an error message
                return;
            }

            string toEmail = employee.Email;

            // Fetch role name from the database using FkRoleId
            string roleName = _dbContext.Roles.FirstOrDefault(r => r.RoleId == employee.fk_role_id)?.RoleName ?? "Unknown Role";

            // Fetch line manager name from the database using LineManagerId
            string lineManagerName = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employee.LineManagerId)?.FullName ?? "Unknown Line Manager";

            // Fetch password from the database
            string password = employee.Password; // Assuming the password is already hashed and stored securely

            string subject = $"Welcome to {companyName}";
            string body = $"Hello {employee.FullName}\n\n"
                          + $"You have been successfully registered as an employee at {companyName}.\n\n"
                          + $"Your details:\n"
                          + $"Email: {employee.Email}\n"
                          + $"Line Manager: {lineManagerName}\n"
                          + $"Password: {generatedPassword}\n\n"
                          + $"Thank you for joining our team!\n\n"
                          + $"Best regards,\n{companyName} Team";

            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                // Set your SMTP credentials
                client.Credentials = new NetworkCredential("dpatidar1221@gmail.com", "lveh yhaw szhz lydf");
                client.EnableSsl = true;
                client.Port = 587;

                // Set the sender and receiver email addresses
                string fromEmail = "dpatidar1221@gmail.com";
                MailAddress from = new MailAddress(fromEmail, "HRMS SUPPORT");

                // Ensure that the toEmail is not empty or null
                if (string.IsNullOrEmpty(toEmail))
                {
                    // Log or handle the error appropriately
                    // You might want to return or log an error message
                    return;
                }

                MailAddress to = new MailAddress(toEmail);

                // Create the email message
                using (var message = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false // Change to true if you want to use HTML in the email body
                })
                {
                    // Send the email
                    client.Send(message);
                }
            }
        }
        private string GenerateRandomPassword()
        {
            // Implement your random password generation logic here
            // For example, use a library or custom logic to generate a random password
            // This is a simple example, you may want to use a more secure approach in a production environment
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool IsValidPassword(string password)
        {
            // Implement your password validation logic here
            // For example, require at least 8 characters, including uppercase and lowercase letters, and numbers
            return !string.IsNullOrEmpty(password) &&
                   password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit);
        }
        [HttpPut("{employeeId}")]
        public IActionResult UpdateEmployee(int employeeId, [FromBody] EmployeeUpdateDto updatedEmployeeDto)
        {
            if (updatedEmployeeDto == null)
            {
                return BadRequest("Invalid data. Employee object is null.");
            }

            try
            {
                var existingEmployee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);

                if (existingEmployee == null)
                {
                    return NotFound($"Employee with ID {employeeId} not found.");
                }

                // Check if the email is being changed to an email that already exists
                if (_dbContext.Employees.Any(e => e.Email == updatedEmployeeDto.Email && e.EmployeeId != employeeId))
                {
                    return BadRequest("An employee with the same email already exists.");
                }

                // Update the existing employee's properties
                existingEmployee.FullName = updatedEmployeeDto.Name;
                existingEmployee.Email = updatedEmployeeDto.Email;
                existingEmployee.LineManagerId = updatedEmployeeDto.LineManagerId;
                existingEmployee.fk_role_id = updatedEmployeeDto.FkRoleId;

                // Save all changes to the database
                _dbContext.SaveChanges();

                return Ok($"Employee updated successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception or return an appropriate error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the employee with ID {employeeId}.");
            }
        }

        public class EmployeeUpdateDto
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public int LineManagerId { get; set; }
            public int FkRoleId { get; set; }
        }

        public enum UserRole
        {
            SuperAdmin = 1,
            // Add other roles as needed
        }


        [HttpGet]
        public IActionResult GetEmployees()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                // Handle the case where company information is not available in the token
                return BadRequest("Invalid data. Company information is missing.");
            }

            var employees = _dbContext.Employees
       .Where(e => e.FkCompanyId == companyId)
       .Include(e => e.FkDepartment)
       .Include(e => e.FkDesignation) // Include FkDesignation here
                  .Include(e => e.FkRole)                      // Eagerly load FkOffice

               .Select(e => new
               {
                   e.EmployeeId,
                   e.CreatedDate,
                   e.CreatedBy,
                   e.ModifiedDate,
                   e.ModifiedBy,
                   e.FkDepartmentId,
                   e.LineManagerId,
                   e.FkCompanyId,
                   e.FkUserId,
                   e.FirstName,
                   e.LastName,
                   e.MobileNo,
                   e.Email,
                   RoleName = e.FkRole != null ? e.FkRole.RoleName : null,
                   e.FullName,
                   e.Gender,
                   e.DateOfBirth,
                   e.MaritalStatus,
                   e.Nationality,
                   e.PersonalTaxId,
                   e.SocialInsurance,
                   e.HealthInsurance,
                   e.PhoneNumber,
                   e.JoinDate,
                   e.MarriageAnniversary,
                   e.AlternateMobileNo,
                   e.IsActive,
                   e.FkEmpstatusId,
                   CompanyName = e.FkCompany != null ? e.FkCompany.CompanyName : null, // Include CompanyName in the response
                   OfficeName = e.FkOffice != null ? e.FkOffice.OfficeName : null,
                   EmployeeStatus = e.FkEmpstatus.StatusName, // Include EmployeeStatus in the response
                   FeaturedImageURL = e.Image, // Assuming FeaturedImage is the file path or URL
                   e.FkLoginHistoryId,
                   e.FkOfficeId,
                   e.FkEmployeeGroupId,
                   DesignationName = e.FkDesignation != null ? e.FkDesignation.DesignationName : null, // Include DesignationName
                   DepartmentName = e.FkDepartment != null ? e.FkDepartment.DepartmentName : null, // Include DepartmentName

                   LineManagerName = e.LineManagerId != null ? _dbContext.Employees
                        .Where(em => em.EmployeeId == e.LineManagerId)
                        .Select(em => em.FullName)
                        .FirstOrDefault() : null
               })
                .ToList();

            return Ok(employees);
        }
        [HttpGet("Paging")]
        public IActionResult GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
                {
                    // Handle the case where company information is not available in the token
                    return BadRequest("Invalid data. Company information is missing.");
                }

                if (page < 1)
                {
                    return BadRequest("Invalid page number. Page number must be greater than or equal to 1.");
                }

                if (pageSize < 1)
                {
                    return BadRequest("Invalid page size. Page size must be greater than or equal to 1.");
                }

                var employeesQuery = _dbContext.Employees
                    .Where(e => e.FkCompanyId == companyId)
                    .Include(e => e.FkDepartment)
                    .Include(e => e.FkDesignation)
                    // Eagerly load FkOffice
                    .OrderBy(e => e.EmployeeId) // Order by a unique field to ensure consistent results for paging
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        e.EmployeeId,
                        e.CreatedDate,
                        e.CreatedBy,
                        e.ModifiedDate,
                        e.ModifiedBy,
                        e.FkDepartmentId,
                        e.LineManagerId,
                        e.FkCompanyId,
                        e.FkUserId,
                        e.FirstName,
                        e.LastName,
                        e.MobileNo,
                        e.Email,
                        RoleName = e.FkRole != null ? e.FkRole.RoleName : null,
                        e.FullName,
                        e.Gender,
                        e.DateOfBirth,
                        e.MaritalStatus,
                        e.Nationality,
                        e.PersonalTaxId,
                        e.SocialInsurance,
                        e.HealthInsurance,
                        e.PhoneNumber,
                        e.JoinDate,
                        e.MarriageAnniversary,
                        e.AlternateMobileNo,
                        e.IsActive,
                        e.FkEmpstatusId,
                        CompanyName = e.FkCompany != null ? e.FkCompany.CompanyName : null, // Include CompanyName in the response
                        OfficeName = e.FkOffice != null ? e.FkOffice.OfficeName : null,
                        EmployeeStatus = e.FkEmpstatus.StatusName, // Include EmployeeStatus in the response
                        FeaturedImageURL = e.Image, // Assuming FeaturedImage is the file path or URL
                        e.FkLoginHistoryId,
                        e.FkOfficeId,
                        e.FkEmployeeGroupId,
                        DesignationName = e.FkDesignation != null ? e.FkDesignation.DesignationName : null, // Include DesignationName
                        DepartmentName = e.FkDepartment != null ? e.FkDepartment.DepartmentName : null, // Include DepartmentName

                        LineManagerName = e.LineManagerId != null ? _dbContext.Employees
                        .Where(em => em.EmployeeId == e.LineManagerId)
                        .Select(em => em.FullName)
                        .FirstOrDefault() : null
                    });


                var totalRecords = _dbContext.Employees.Count(e => e.FkCompanyId == companyId);

                var response = new
                {
                    TotalRecords = totalRecords,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    Employees = employeesQuery.ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            { 
                // Log the exception for troubleshooting
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("getemployee{employeeId}")]
        public IActionResult GetEmployeeDetailsWithid(int employeeId)
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                // Handle the case where company information is not available in the token
                return BadRequest("Invalid data. Company information is missing.");
            }

            var employees = _dbContext.Employees
      .Where(e => e.FkCompanyId == companyId)
      .Include(e => e.FkDepartment)
      .Include(e => e.FkDesignation) // Include FkDesignation here

              .Select(e => new
              {
                  e.EmployeeId,
                  e.CreatedDate,
                  e.CreatedBy,
                  e.ModifiedDate,
                  e.ModifiedBy,
                  e.FkDepartmentId,
                  e.LineManagerId,
                  e.FkCompanyId,
                  e.FkUserId,
                  e.FirstName,
                  e.LastName,
                  e.MobileNo,
                  e.Email,
              
                  e.FullName,
                  e.JoinDate,
                  e.Gender,
                  e.DateOfBirth,
                  e.MaritalStatus,
                  e.Nationality,
                  e.PersonalTaxId,
                  e.SocialInsurance,
                  e.HealthInsurance,
                  e.PhoneNumber,
                  e.MarriageAnniversary,
                  e.AlternateMobileNo,
                  e.IsActive,
                  e.FkEmpstatusId,
                  OfficeName = e.FkOffice != null ? e.FkOffice.OfficeName : null,
                  EmployeeStatus = e.FkEmpstatus.StatusName, // Include EmployeeStatus in the response
                  FeaturedImageURL = e.Image, // Assuming FeaturedImage is the file path or URL
                  e.FkLoginHistoryId,
                  e.FkOfficeId,
                  e.FkEmployeeGroupId,
                  DesignationName = e.FkDesignation != null ? e.FkDesignation.DesignationName : null, // Include DesignationName
                  DepartmentName = e.FkDepartment != null ? e.FkDepartment.DepartmentName : null, // Include DepartmentName

                  LineManagerName = e.LineManagerId != null ? _dbContext.Employees
                        .Where(em => em.EmployeeId == e.LineManagerId)
                        .Select(em => em.FullName)
                        .FirstOrDefault() : null
              })
                .FirstOrDefault();

            if (employees == null)
            {
                return NotFound("Employee not found.");
            }

            return Ok(employees);
        }
        [HttpDelete("delete/{employeeId}")]
        public async Task<IActionResult> DeleteEmployee(int employeeId)
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                // Handle the case where company information is not available in the token
                return BadRequest("Invalid data. Company information is missing.");
            }

            try
            {
                var employee = await _dbContext.Employees.FindAsync(employeeId);

                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                // Load the associated role using the FKRoleId
                var role = await _dbContext.Roles.FindAsync(employee.fk_role_id);

                if (role != null && role.RoleName == "Superadmin")
                {
                    return BadRequest("Cannot delete an employee with the role 'Superadmin'.");
                }

                _dbContext.Employees.Remove(employee);
                await _dbContext.SaveChangesAsync();

                return Ok("Employee deleted successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging purposes
                Console.WriteLine(ex.ToString());

                // Handle the exception appropriately (e.g., log the error or return an error response)
                return StatusCode(500, "An error occurred while deleting the employee.");
            }
        }


        [HttpGet("{employeeId}")]
        public IActionResult GetProfilePageSide(int employeeId)
        {
            var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            var profilePageSide = new
            {
                FullName = employee.FullName,
                Email = employee.Email,
                Image = Convert.ToBase64String(employee.Fimage ?? Array.Empty<byte>())
            };

            return Ok(profilePageSide);
        }
        [HttpGet("department-designation-list")]
        public IActionResult GetDepartmentDesignationRoleList()
        {
            // Retrieve all departments from the database
            var departments = _dbContext.Departments
                .Select(d => new
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName
                })
                .ToList();

            // Retrieve all designations from the database
            var designations = _dbContext.Designations
                .Select(des => new
                {
                    DesignationId = des.DesignationId,
                    DesignationName = des.DesignationName
                })
                .ToList();

            // Retrieve all roles from the database

            // Create a response object containing data for departments, designations, and roles
            var response = new
            {
                Departments = departments,
                Designations = designations

            };

            // Return the response as a JSON object
            return Ok(response);
        }


        [HttpPost("import")]
        public IActionResult ImportEmployees(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("Invalid file");
            }

            // Set the license context to suppress the license exception
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = file.OpenReadStream())
            {
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                    {
                        return BadRequest("Invalid worksheet");
                    }

                    // Process the data from the worksheet
                    var employees = ProcessWorksheet(worksheet);

                    // Insert data into the database
                    InsertDataIntoDatabase(employees);

                    return Ok("Import successful");
                }
            }
        }

        private List<EmployeeDto> ProcessWorksheet(ExcelWorksheet worksheet)
        {
            var employees = new List<EmployeeDto>();

            // Determine the last row with valid data
            int lastDataRow = worksheet.Dimension.Rows;
            for (int row = worksheet.Dimension.Rows; row >= 1; row--)
            {
                // Check if key columns have data (assuming columns 1, 3, and 5 are key columns)
                if (!string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) &&
                    !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) &&
                    !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text))
                {
                    lastDataRow = row;
                    break;
                }
            }

            // Process the data up to the last row with valid data
            for (int row = 2; row <= lastDataRow; row++)
            {
                // Check if key columns have data
                if (!string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) &&
                    !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) &&
                    !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text))
                {
                    var employee = new EmployeeDto
                    {
                        FullName = worksheet.Cells[row, 1].GetValue<string>(),
                        DepartmentName = worksheet.Cells[row, 3].GetValue<string>(),
                        JobTitle = worksheet.Cells[row, 4].GetValue<string>(),
                        LineManagerName = worksheet.Cells[row, 5].GetValue<string>(),
                    /*    CreatedDate = worksheet.Cells[row, 6].GetValue<DateTime?>(),
                        CreatedBy = worksheet.Cells[row, 7].GetValue<string>(),
                        ModifiedDate = worksheet.Cells[row, 8].GetValue<DateTime?>(),
                        ModifiedBy = worksheet.Cells[row, 9].GetValue<string>(),*/
                        Email = worksheet.Cells[row, 2].GetValue<string>(),
                       /* Password = worksheet.Cells[row, 11].GetValue<string>(),
                        // Add other fields accordingly*/
                    };

                    employees.Add(employee);
                }
            }

            return employees;
        }

        private void InsertDataIntoDatabase(List<EmployeeDto> employees)
        {
            foreach (var employeeDto in employees)
            {
                // Insert Department if not exists
                var department = _dbContext.Departments
                    .FirstOrDefault(d => d.DepartmentName == employeeDto.DepartmentName);

                if (department == null)
                {
                    department = new Department
                    {
                        DepartmentId = GetNextDepartmentId(), // Increment DepartmentId
                        DepartmentName = employeeDto.DepartmentName,
                        // Map other properties accordingly
                    };

                    _dbContext.Departments.Add(department);
                    _dbContext.SaveChanges(); // Save changes to get the DepartmentId
                }

                // Insert Designation if not exists
                var designation = _dbContext.Designations
                    .FirstOrDefault(d => d.DesignationName == employeeDto.JobTitle);

                if (designation == null)
                {
                    designation = new Designation
                    {
                        DesignationId = GetNextDesignationId(), // Increment DesignationId
                        DesignationName = employeeDto.JobTitle,
                        // Map other properties accordingly
                    };

                    _dbContext.Designations.Add(designation);
                    _dbContext.SaveChanges(); // Save changes to get the DesignationId
                }

                // Insert Line Manager if exists
                int? lineManagerId = null;
                if (!string.IsNullOrEmpty(employeeDto.LineManagerName))
                {
                    var lineManager = _dbContext.Employees
                        .FirstOrDefault(e => e.FullName == employeeDto.LineManagerName);

                    if (lineManager != null)
                    {
                        lineManagerId = lineManager.EmployeeId;
                    }
                }
                string generatedPassword = GenerateRandomPassword();
                // Insert Employee
                var newEmployee = new Employee
        {
            EmployeeId = GetNextEmployeeId(), // Increment EmployeeId
            FullName = employeeDto.FullName,
            FkDepartmentId = department.DepartmentId,
            FkCompanyId = GetCompanyIdFromToken(), // Get CompanyId from token
            LineManagerId = lineManagerId,
            FkDesignationId = designation.DesignationId,
            CreatedDate = employeeDto.CreatedDate,
            CreatedBy = "system",
            ModifiedDate = null,
            ModifiedBy = null,
            Email = employeeDto.Email,
            Password = generatedPassword, // Save the plain generated password
                    fk_role_id = GetRoleIdByNameAndCompany("Employee", GetCompanyIdFromToken()),
                    // Add other properties accordingly
                };

                _dbContext.Employees.Add(newEmployee);
                _dbContext.SaveChanges();

                SendEmail(newEmployee, generatedPassword);
            }
        }
        private int GetRoleIdByNameAndCompany(string roleName, int companyId)
        {
            var role = _dbContext.Roles
                .FirstOrDefault(r => r.RoleName == roleName && r.CompanyId == companyId);

            if (role != null)
            {
                return role.RoleId;
            }

            // Handle the case when the role is not found
            // You can throw an exception, return a default role ID, or handle it based on your application logic
            // For example, return a default role ID of 0
            return 0;
        }


        private int GetNextEmployeeId()
        {
            // Implement logic to get the next unique EmployeeId
            // This could involve querying the database for the maximum existing EmployeeId
            // and incrementing it.
            // For simplicity, you can use a counter, but in a real-world scenario, database-generated IDs are preferable.
            return _dbContext.Employees.Max(e => (int?)e.EmployeeId) + 1 ?? 1;
        }

        private int GetNextDepartmentId()
        {
            // Implement logic to get the next unique DepartmentId
            // This could involve querying the database for the maximum existing DepartmentId
            // and incrementing it.
            // For simplicity, you can use a counter, but in a real-world scenario, database-generated IDs are preferable.
            return _dbContext.Departments.Max(d => (int?)d.DepartmentId) + 1 ?? 1;
        }
        private int GetNextDesignationId()
        {
            // Implement logic to get the next unique DesignationId
            // This could involve querying the database for the maximum existing DesignationId
            // and incrementing it.
            // For simplicity, you can use a counter, but in a real-world scenario, database-generated IDs are preferable.

            // Generate a new unique identifier for DesignationId
            return _dbContext.Designations.Max(d => (int?)d.DesignationId) + 1 ?? 1;
        }

        private int GetCompanyIdFromToken()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                // Handle the case where company information is not available in the token
                throw new InvalidOperationException("Invalid data. Company information is missing.");
            }

            return companyId;
        }
        public class EmployeeDto
        {
            public string FullName { get; set; }
            public string LineManagerName { get; set; }
            public string JobTitle { get; set; }
            public string DepartmentName { get; set; }
            public DateTime? CreatedDate { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public string ModifiedBy { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            // Add other properties accordingly
        }

    }
}
