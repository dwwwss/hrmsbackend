
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using hrms_backend.Models;
using System.Security.Claims;
using hrms_backend;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace Hrms_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimesoffController : ControllerBase
    {
        private readonly ILogger<TimesoffController> _logger;
        private readonly HrmsDbContext _context;
        private readonly IOptions<EmailConfiguration> _emailConfigOptions;

        public TimesoffController(ILogger<TimesoffController> logger, HrmsDbContext context, IOptions<EmailConfiguration> emailConfigOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailConfigOptions = emailConfigOptions ?? throw new ArgumentNullException(nameof(emailConfigOptions));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLeaves()
        {
            // Get the CompanyId and EmployeeId claims from the user
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId) ||
                employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int employeeId))
            {
                // Handle the case where company or employee information is not available in the token
                return BadRequest("Invalid data. Company or employee information is missing.");
            }

            var leavesQuery = _context.Leaves
                .Join(_context.Employees.Where(e => e.FkCompanyId == companyId),
                    l => l.FkEmployeeId,
                    e => e.EmployeeId,
                    (l, e) => new
                    {
                        l.LeaveId,
                        FromDate = l.FromDate.HasValue ? l.FromDate.Value.ToString("d MMM yyyy") : null,
                        ToDate = l.ToDate.HasValue ? l.ToDate.Value.ToString("d MMM yyyy") : null,
                        l.Attachment,
                        LeaveTypeName = _context.LeaveTypes.FirstOrDefault(lt => lt.LeaveTypeId == l.FkLeaveTypeId) != null ?
                                        _context.LeaveTypes.FirstOrDefault(lt => lt.LeaveTypeId == l.FkLeaveTypeId).Name : null,
                        StatusName = MapStatusIdToStatusName((int)l.Status), // Map StatusId to StatusName
                        FullName = e.FullName,
                        FeaturedImage = e.FeaturedImage,
                        LineManagerId = e.LineManagerId,
                        FkCompanyId = e.FkCompanyId
                    });

            if (roleClaim != null && roleClaim.Value == "Superadmin")
            {
                // If the user is a Superadmin, show all leaves for the company
                var leaves = await leavesQuery.ToListAsync();
                return Ok(leaves);
            }
            else
            {
                // Otherwise, show leaves based on the condition (CompanyId == FkCompanyId && EmployeeId == LineManagerId)
                var leaves = await leavesQuery
                    .Where(l => l.FkCompanyId.HasValue && l.FkCompanyId == companyId &&
                                l.LineManagerId.HasValue && l.LineManagerId == employeeId)
                    .ToListAsync();

                return Ok(leaves);
            }
        }
        [HttpGet("leaves/{employeeId}")]
        public async Task<ActionResult<object>> GetLeavesByEmployeeId(int employeeId)
        {
            // Get the CompanyId claim from the user
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                // Handle the case where company information is not available in the token
                return BadRequest("Invalid data. Company information is missing.");
            }

            var leaves = await _context.Leaves
                .Where(l => l.FkEmployeeId == employeeId)
                .Select(l => new
                {
                    l.LeaveId,
                    FromDate = l.FromDate.HasValue ? l.FromDate.Value.ToString("d MMM yyyy") : null,
                    ToDate = l.ToDate.HasValue ? l.ToDate.Value.ToString("d MMM yyyy") : null,
                    l.Attachment,
                    LeaveTypeName = _context.LeaveTypes.FirstOrDefault(lt => lt.LeaveTypeId == l.FkLeaveTypeId) != null ?
                                    _context.LeaveTypes.FirstOrDefault(lt => lt.LeaveTypeId == l.FkLeaveTypeId).Name : null,
                    Status = l.Status // Assuming Status is an integer in the Leaf table
                })
                .ToArrayAsync();

            if (leaves == null || leaves.Length == 0)
            {
                return NotFound();
            }

            // Map Status to corresponding status names
            var result = leaves.Select(leave => new
            {
                leave.LeaveId,
                leave.FromDate,
                leave.ToDate,
                leave.Attachment,
                leave.LeaveTypeName,
                Status = MapStatusIdToStatusName((int)leave.Status) // Use Status directly
            }).ToArray();

            return Ok(result);
        }

        private static string MapStatusIdToStatusName(int statusId)
        {
            switch (statusId)
            {
                case 1:
                    return "Pending";
                case 2:
                    return "Approved";
                case 3:
                    return "Rejected";
                default:
                    return "Unknown";
            }
        }

        [HttpPost("CreateLeave")]
        public async Task<IActionResult> CreateLeafWithLeaveType([FromBody] LeafRequest model)
        {
            try
            {
                // Check if the specified FkLeaveTypeId exists
                var leaveType = await _context.LeaveTypes.FirstOrDefaultAsync(lt => lt.LeaveTypeId == model.FkLeaveTypeId);
                if (leaveType == null)
                {
                    _logger.LogInformation($"Invalid FkLeaveTypeId. Leave type not found. FkLeaveTypeId: {model.FkLeaveTypeId}");
                    return BadRequest("Invalid FkLeaveTypeId. Leave type not found.");
                }

                // Obtain the employee ID from the claims in the user's identity
                if (!int.TryParse(User.FindFirstValue("EmployeeId"), out int fkEmployeeId))
                {
                    _logger.LogInformation($"Invalid or missing employee ID in the token.");
                    return BadRequest("Invalid or missing employee ID in the token.");
                }

                // Check eligibility based on FkLeaveTypeId and fkEmployeeId
                if (!await IsLeaveTypeEligible(fkEmployeeId, model.FkLeaveTypeId))
                {
                    _logger.LogInformation($"Leave type is not eligible for the specified conditions. Employee ID: {fkEmployeeId}, Leave Type ID: {model.FkLeaveTypeId}");
                    return BadRequest("Leave type is not eligible for the specified conditions.");
                }
                // Assuming you have the Workschedule entity for the employee
                var employee = await _context.Employees
      .FirstOrDefaultAsync(e => e.EmployeeId == fkEmployeeId);

                if (employee != null && employee.FkScheduleId != null)
                {
                    // Fetch WorkSchedule using fkWorkScheduleId
                    var workSchedule = await _context.Workschedules
                        .FirstOrDefaultAsync(ws => ws.ScheduleId == employee.FkScheduleId);

                    if (workSchedule != null)
                    {
                        // Calculate total days including non-working days
                        var totalDays = (model.ToDate - model.FromDate).Days + 1; // Including the end date
                        var workingDaysObject = JsonConvert.DeserializeObject<Dictionary<string, bool>>(workSchedule.WorkingDays);

                        // Add non-working days between FromDate and ToDate
                        var nonWorkingDays = Enumerable.Range(0, totalDays)
                            .Select(offset => model.FromDate.AddDays(offset).DayOfWeek)
                            .Count(dayOfWeek => !workingDaysObject[dayOfWeek.ToString("d")]);

                        var workingDays = totalDays - nonWorkingDays;

                        // Create a new Leaf entity
                        var lastLeaveId = _context.Leaves.Max(leave => (int?)leave.LeaveId) ?? 0;
                        var seedId = lastLeaveId + 1;

                        var newLeaf = new Leaf
                        {
                            LeaveId = seedId,
                            FromDate = model.FromDate,
                            ToDate = model.ToDate,
                            Attachment = model.Attachment,
                            Status = 1, // Assuming Status ID 1
                            IsActive = true,
                            FkLeaveTypeId = model.FkLeaveTypeId,
                            Note = model.Note,
                            total_days = workingDays,
                            FkEmployeeId = fkEmployeeId

                            // Other properties...
                        };


                        // Add the new leaf to the context and save changes
                        _context.Leaves.Add(newLeaf);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Leaf created successfully.");

                        var lineManagerId = employee.LineManagerId; // Assuming LineManagerId is a property in the Employee entity

                        if (lineManagerId != null)
                        {
                            var lineManager = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == lineManagerId);

                            if (lineManager != null && !string.IsNullOrEmpty(lineManager.Email))
                            {
                                // Prepare and send the email
                                var emailSubject = "Leave Request";
                                var emailBody = $"Dear {lineManager.FullName},\n\n" +
                                                $"Employee with ID {fkEmployeeId} has requested leave.\n" +
                                                $"Please review and approve/deny the request.\n\n" +
                                                "HRMS Support";

                                SendEmail(lineManager.Email, emailSubject, emailBody);
                            }
                            else
                            {
                                _logger.LogWarning("Line manager's email not found.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Line manager ID not found.");
                        }

                        return Ok("Leaf created successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging purposes
                _logger.LogError($"An error occurred: {ex}");

                // Handle the exception appropriately (e.g., log the error or return an error response)
                return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
            }

            // Add a return statement outside the try block to satisfy the compiler
            return BadRequest("Unexpected error occurred during leave creation.");
        }

        private async Task<bool> IsLeaveTypeEligible(int fkEmployeeId, int fkLeaveTypeId)
        {
            try
            {
                var employee = await _context.Employees
                    .Where(e => e.EmployeeId == fkEmployeeId)
                    .FirstOrDefaultAsync();

                var eligibleLeaveType = await _context.LeaveTypes
                    .Where(lt => lt.LeaveTypeId == fkLeaveTypeId && (lt.IsActive ?? true))
                    .FirstOrDefaultAsync();

                if (employee == null || eligibleLeaveType == null)
                {
                    _logger.LogError($"Employee or LeaveType not found. EmployeeId: {fkEmployeeId}, LeaveTypeId: {fkLeaveTypeId}");
                    return false;
                }

                _logger.LogInformation($"Employee Employment Type: {employee.FkEmployementTypeId}");
                _logger.LogInformation($"Employee ID: {employee.EmployeeId}");
                _logger.LogInformation($"Eligible Leave Type ID: {eligibleLeaveType.LeaveTypeId}");
                _logger.LogInformation($"Eligibility Check - IsActive: {(eligibleLeaveType.IsActive ?? true)}");

                var isEmployeeTypeEligible = IsEmployeeTypeEligible(employee, eligibleLeaveType);
                var isEmployeeInSpecificList = IsEmployeeInSpecificList(employee.EmployeeId, eligibleLeaveType.SpecificEmployees);

                _logger.LogInformation($"Eligibility Check Results - EmployeeType: {isEmployeeTypeEligible}, InSpecificList: {isEmployeeInSpecificList}");

                return isEmployeeTypeEligible || isEmployeeInSpecificList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during eligibility check: {ex}");
                return false;
            }
        }


        private bool IsEmployeeTypeEligible(Employee employee, LeaveType eligibleLeaveType)
        {
            if (!string.IsNullOrEmpty(eligibleLeaveType.EligibleEmployeeType))
            {
                var eligibleEmployeeTypes = eligibleLeaveType.EligibleEmployeeType
                     .Trim('{', '}')
                    .Split(',')
                    .Where(s => int.TryParse(s.Trim(), out _))
                    .Select(s => int.Parse(s.Trim()))
                    .ToList();

                _logger.LogInformation($"Employee Employment Type: {employee.FkEmployementTypeId}");
                _logger.LogInformation($"Eligible Employee Types: {string.Join(", ", eligibleEmployeeTypes)}");

                return eligibleEmployeeTypes.Any(e => e == (employee.FkEmployementTypeId ?? 0));
            }
            else
            {
                _logger.LogInformation("Eligible Employee Types is empty or null.");
                return false;
            }
        }


        private bool IsEmployeeInSpecificList(int employeeId, string specificEmployees)
        {
            if (!string.IsNullOrEmpty(specificEmployees))
            {
                var specificEmployeeIds = specificEmployees
                    .Trim('{', '}')
                    .Split(',')
                    .Where(s => int.TryParse(s.Trim(), out _))
                    .Select(s => int.Parse(s.Trim()))
                    .ToList();

                _logger.LogInformation($"Employee ID: {employeeId}");
                _logger.LogInformation($"Specific Employees: {specificEmployees}");

                return specificEmployeeIds.Contains(employeeId);
            }
            else
            {
                // Agar specificEmployees null hai, to false return karo.
                return false;
            }
        }


        public class LeafRequest
        {
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public string? Attachment { get; set; }
            public int? Status { get; set; }
            public bool? IsActive { get; set; }
            public int FkLeaveTypeId { get; set; }
            public string Note { get; set; }
            public int FkEmployeeId { get; set; }
            // Other properties...
        }

        [HttpPost]
        public IActionResult CreateLeaveType([FromBody] LeaveTypeRequest model)
        {
            // Retrieve company ID from the user's token claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            // Ensure that the company ID claim exists
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return BadRequest("Invalid or missing company ID in the token.");
            }

            // Convert arrays of integers to strings for database storage
            string eligibleEmployeeTypeString = "{" + string.Join(",", model.EligibleEmployeeType) + "}";
            string specificEmployeesString = "{" + string.Join(",", model.SpecificEmployees) + "}";

            // Find the maximum existing LeaveTypeId in the database
            int nextLeaveTypeId = _context.LeaveTypes.Any() ? _context.LeaveTypes.Max(lt => lt.LeaveTypeId) + 1 : 1;


            // Create the leave type
            var leaveType = new LeaveType
            {
                LeaveTypeId = nextLeaveTypeId,
                Name = model.Name,
                Description = model.Description,
                IsPaid = model.IsPaid,
                Count = model.Count,
                Duration = model.Duration,
                Limit = model.Limit,
                IsCarryForward = model.IsCarryForward,
                ExpiryDate = model.ExpiryDate,
                EligibleEmployeeType = eligibleEmployeeTypeString,
                SpecificEmployees = specificEmployeesString,
                FkCompanyId = companyId  // Set the FkCompanyId
                                         // Set other properties as needed
            };

            _context.LeaveTypes.Add(leaveType);
            _context.SaveChanges();
            return Ok("Leave type created successfully.");
        }
        [HttpPut("{leaveTypeId}")]
        public IActionResult EditLeaveType(int leaveTypeId, [FromBody] LeaveTypeRequest model)
        {
            // Retrieve company ID from the user's token claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            // Ensure that the company ID claim exists
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return BadRequest("Invalid or missing company ID in the token.");
            }

            // Find the leave type in the database
            var leaveType = _context.LeaveTypes.FirstOrDefault(lt => lt.LeaveTypeId == leaveTypeId && lt.FkCompanyId == companyId);

            // Check if the leave type exists
            if (leaveType == null)
            {
                return NotFound("Leave type not found.");
            }

            // Update the leave type properties
            leaveType.Name = model.Name;
            leaveType.Description = model.Description;
            leaveType.IsPaid = model.IsPaid;
            leaveType.Count = model.Count;
            leaveType.Duration = model.Duration;
            leaveType.Limit = model.Limit;
            leaveType.IsCarryForward = model.IsCarryForward;
            leaveType.ExpiryDate = model.ExpiryDate;

            // Convert arrays of integers to strings for database storage
            string eligibleEmployeeTypeString = "{" + string.Join(",", model.EligibleEmployeeType) + "}";
            string specificEmployeesString = "{" + string.Join(",", model.SpecificEmployees) + "}";

            leaveType.EligibleEmployeeType = eligibleEmployeeTypeString;
            leaveType.SpecificEmployees = specificEmployeesString;

            // Save changes to the database
            _context.SaveChanges();

            return Ok("Leave type updated successfully.");
        }

        [HttpDelete("{leaveTypeId}")]
        public IActionResult DeleteLeaveType(int leaveTypeId)
        {
            // Retrieve company ID from the user's token claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            // Ensure that the company ID claim exists
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return BadRequest("Invalid or missing company ID in the token.");
            }
            var leavesToDelete = _context.Leaves.Where(l => l.FkLeaveTypeId == leaveTypeId);
            _context.Leaves.RemoveRange(leavesToDelete);
            _context.SaveChanges();

            // Find the leave type in the database
            var leaveType = _context.LeaveTypes
                .FirstOrDefault(lt => lt.LeaveTypeId == leaveTypeId && lt.FkCompanyId == companyId);

            // Check if the leave type exists
            if (leaveType == null)
            {
                return NotFound("Leave type not found.");
            }

            // Remove the leave type from the database
            _context.LeaveTypes.Remove(leaveType);
            _context.SaveChanges();

            return Ok("Leave type deleted successfully.");
        }


        [HttpGet]
        [Route("GetLeaveTypes")]
        public IActionResult GetLeaveTypes()
        {
            // Retrieve company ID from the user's token claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            // Ensure that the company ID claim exists
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return BadRequest("Invalid or missing company ID in the token.");
            }

            // Retrieve leave types for the specified company
            var leaveTypes = _context.LeaveTypes
                .Where(lt => lt.FkCompanyId == companyId)
                .ToList();

            // Map eligibleEmployeeType values to corresponding names
            foreach (var leaveType in leaveTypes)
            {
                leaveType.EligibleEmployeeType = MapEmployeeTypeNames(leaveType.EligibleEmployeeType);
            }

            return Ok(leaveTypes);
        }

        // Mapping function for eligibleEmployeeType values
        private string MapEmployeeTypeNames(string eligibleEmployeeType)
        {
            if (string.IsNullOrWhiteSpace(eligibleEmployeeType))
            {
                return "Others"; // Default value for null or empty
            }

            // Assuming eligibleEmployeeType is in the format "{1,2,3}"
            var employeeTypes = eligibleEmployeeType
                .Trim('{', '}')
                .Split(',')
                .Select(type => MapSingleEmployeeType(type.Trim()));

            return string.Join(", ", employeeTypes);
        }

        // Mapping function for a single employee type
        private string MapSingleEmployeeType(string type)
        {
            switch (type)
            {
                case "1":
                    return "Full Time";
                case "2":
                    return "Part Time";
                case "3":
                    return "Probation";
                default:
                    return "Others"; // Default for unknown values
            }
        }

        public class LeaveTypeRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsPaid { get; set; }
            public int Count { get; set; }
            public string Duration { get; set; }
            public int Limit { get; set; }
            public bool IsCarryForward { get; set; }
            public DateTime ExpiryDate { get; set; }
            public List<int> EligibleEmployeeType { get; set; }
            public List<int> SpecificEmployees { get; set; }
        }
        [HttpGet]
        [Route("GetTotalLeaveCount/{leaveTypeId}")]
        public IActionResult GetTotalLeaveCount(int leaveTypeId)
        {
            // Retrieve company ID from the user's token claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

            // Ensure that the company ID claim and employee ID claim exist
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId) ||
                employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int employeeId))
            {
                return BadRequest("Invalid or missing company ID or employee ID in the token.");
            }

            // Retrieve the specific leave type for the specified company
            var leaveType = _context.LeaveTypes
                .FirstOrDefault(lt => lt.FkCompanyId == companyId && lt.LeaveTypeId == leaveTypeId);

            if (leaveType == null)
            {
                return NotFound($"Leave type with ID {leaveTypeId} not found for the specified company.");
            }

            // Retrieve the total leave count for the specified leave type for the specific employee
            var totalLeaveCount = _context.Leaves
                .Where(l => l.FkLeaveTypeId == leaveType.LeaveTypeId && l.FkEmployeeId == employeeId && l.total_days.HasValue && l.Status != 3 && l.Status != 1)
                .Sum(l => l.total_days.Value);

            // Subtract the total leave count from the Count property of the LeaveType
            var remainingLeaveCount = (int)(leaveType.Count - totalLeaveCount);

            // Create a response object with leave type information and remaining count
            var leaveTypeCountResponse = new LeaveTypeCountResponse
            {
                LeaveTypeId = leaveType.LeaveTypeId,
                LeaveTypeName = leaveType.Name,
                RemainingLeaveCount = remainingLeaveCount
            };

            return Ok(leaveTypeCountResponse);
        }


        public class LeaveTypeCountResponse
        {
            public int? LeaveTypeId { get; set; }
            public string? LeaveTypeName { get; set; }
            public int? RemainingLeaveCount { get; set; }
        }





        [HttpPut("updatestatus/{leaveId}")]
        public IActionResult UpdateLeaveStatus(int leaveId, [FromQuery(Name = "newStatusId")] int newStatusId)
        {
            try
            {
                var leave = _context.Leaves.Include(l => l.FkEmployee).FirstOrDefault(l => l.LeaveId == leaveId);

                if (leave == null)
                {
                    return NotFound("Leave not found.");
                }

                // Update leave status
                leave.Status = newStatusId;
                _context.SaveChanges();

                var employeeEmail = leave.FkEmployee.Email;

                // Send congratulatory email
                string subject = "Congratulations! Your Leave Status has been Updated";
                string body = $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: 'Arial', sans-serif;
                        background-color: #F4F4F4;
                        padding: 20px;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: #FFFFFF;
                        padding: 20px;
                        border-radius: 10px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    p {{
                        color: #555555;
                        line-height: 1.6;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <p>Dear {leave.FkEmployee.FullName},</p>
                    <p>Congratulations! Your leave request  has been updated.</p>
                   
                    <p>Best regards,<br>HRMS Team</p>
                </div>
            </body>
            </html>
        ";

                SendEmail(employeeEmail, subject, body);

                return Ok("Leave status updated successfully, and congratulatory email sent.");
            }
            catch (Exception ex)
            {
                // Log the exception details or return the actual exception message
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            EmailConfiguration emailConfig = _emailConfigOptions.Value;

            // Access configuration values
            string smtpServer = emailConfig.SmtpServer;
            int smtpPort = emailConfig.SmtpPort;
            string smtpUsername = emailConfig.SmtpUsername;
            string smtpPassword = emailConfig.SmtpPassword;
            bool enableSsl = emailConfig.EnableSsl;
            string fromEmail = emailConfig.FromEmail;
            string fromDisplayName = emailConfig.FromDisplayName;

            using (MailMessage mailMessage = new MailMessage())
            {
                MailAddress fromAddress = new MailAddress(fromEmail, fromDisplayName);
                mailMessage.From = fromAddress;

                // Set the "Reply-To" address that recipients will see
                mailMessage.ReplyToList.Add(new MailAddress("supporthrms@gmail.com", fromDisplayName));

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
                        Console.WriteLine($"Email sent to {toEmail}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email: {ex.Message}");
                        // Handle the error appropriately (e.g., log it or inform the user)
                    }
                }
            }
        }
    }
}







