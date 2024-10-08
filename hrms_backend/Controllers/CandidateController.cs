using hrms_backend.Models;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using iText.Kernel.Pdf;


namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;
        private readonly string BaseURL;
        public CandidateController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
            BaseURL = "http://10.0.0.168/hrms";
        }
        [HttpPost]
        public async Task<IActionResult> PostCandidate([FromForm] Candidate candidate, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Validate if the candidate with the same email already exists
                if (_dbContext.Candidates.Any(c => c.Email == candidate.Email))
                {
                    ModelState.AddModelError(nameof(candidate.Email), "Candidate with the same email already exists.");
                    return BadRequest(ModelState);
                }

                // Handle file upload
                if (file != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);

                        // Generate a unique file name (you can use Guid.NewGuid() or any other method)
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;

                        // Save the file to the wwwroot/images folder
                        var filePath = Path.Combine("wwwroot", "images", uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            memoryStream.CopyTo(fileStream);
                        }

                        // Set the CV URL in the candidate object
                        candidate.Resume = $"{BaseURL}/images/{uniqueFileName}";
                    }
                }

                // Set the creation date to a specific date format
                candidate.CreatedDate = new DateTime(2023, 12, 27);

                candidate.CreatedBy = GetEmployeeFullName();
                candidate.FkCompanyId = loggedInCompanyId;

                // Set the default stage name to "Applied"
                candidate.StagesName = "Applied";

                _dbContext.Candidates.Add(candidate);
                await _dbContext.SaveChangesAsync();

                return Ok("Candidate added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private string GetEmployeeFullName()
        {
            // Assuming you have access to the EmployeeId from the user claims
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");
            if (employeeIdClaim != null && int.TryParse(employeeIdClaim.Value, out int employeeId))
            {
                var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
                if (employee != null)
                {
                    return employee.FullName;
                }
            }

            // Default to the username if employee information is not available
            return User.Identity.Name;
        }

        public class UpdateStageNameRequest
        {
            public string UpdatedStageName { get; set; }
        }

        [HttpPut("UpdateStageName/{id}")]
        public async Task<IActionResult> UpdateStageName(int id, [FromBody] UpdateStageNameRequest requestBody)
        {
            try
            {
                if (requestBody == null || string.IsNullOrEmpty(requestBody.UpdatedStageName))
                {
                    return BadRequest("Invalid request body. 'UpdatedStageName' is missing or empty.");
                }

                var existingCandidate = await _dbContext.Candidates.FindAsync(id);

                if (existingCandidate == null)
                    return NotFound();

                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Check if the user has access to the specified company
                if (existingCandidate.FkCompanyId != loggedInCompanyId)
                    return Forbid(); // User doesn't have access to the specified company

                // Update the StageName property
                existingCandidate.StagesName = requestBody.UpdatedStageName;

                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Stages name updated successfully" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet]
        public IActionResult GetCandidates()
        {
            try
            {
                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Retrieve candidates with associated job names for the specific company
                var candidates = _dbContext.Candidates
                    .Where(c => c.FkCompanyId == loggedInCompanyId)
                    .Select(c => new
                    {
                        CandidateId = c.CandidateId,
                        Fullname = c.Fullname,
                        Email = c.Email,
                        MobileNo = c.MobileNo,
                        Resume = c.Resume,
                        CreatedDate = c.CreatedDate.HasValue ? c.CreatedDate.Value.ToString("d MMM yyyy") : (string)null,
                        CreatedBy = c.CreatedBy,
                        JoinDate = c.JoinDate.HasValue ? c.JoinDate.Value.ToString("d MMM yyyy") : (string)null,
                        StagesName = c.StagesName,
                        FkCompanyId = c.FkCompanyId,
                        FkJobId = c.FkJobId,
                        Source = c.Source,
                        FkJobName = _dbContext.Jobs
                            .Where(j => j.JobId == c.FkJobId)
                            .Select(j => j.JobTittle)
                            .FirstOrDefault()
                    })
                    .ToList();

                return Ok(candidates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("GetCandidateWithJob")]
        public IActionResult GetCandidates(int jobId)
        {
            try
            {
                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Retrieve candidates with the specified JobId for the specific company
                var candidates = _dbContext.Candidates
                    .Where(c => c.FkCompanyId == loggedInCompanyId && c.FkJobId == jobId)
                    .Select(c => new
                    {
                        CandidateId = c.CandidateId,
                        Fullname = c.Fullname,
                        Email = c.Email,
                        MobileNo = c.MobileNo,
                        Resume = c.Resume,
                        CreatedDate = c.CreatedDate.HasValue ? c.CreatedDate.Value.ToString("d MMM yyyy") : (string)null,
                        CreatedBy = c.CreatedBy,
                        JoinDate = c.JoinDate.HasValue ? c.JoinDate.Value.ToString("d MMM yyyy") : (string)null,
                        StagesName = c.StagesName,
                        FkCompanyId = c.FkCompanyId,
                        FkJobId = c.FkJobId,
                        Source = c.Source,
                        Job = new
                        {
                            JobId = c.FkJobId,
                            JobTitle = _dbContext.Jobs
                                .Where(j => j.JobId == c.FkJobId)
                                .Select(j => j.JobTittle)
                                .FirstOrDefault()
                        }
                    })
                    .ToList();

                return Ok(candidates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            try
            {
                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Retrieve the candidate from the database
                var candidate = await _dbContext.Candidates.FindAsync(id);

                if (candidate == null)
                    return NotFound(); // Candidate not found

                // Check if the user has access to the specified company
                if (candidate.FkCompanyId != loggedInCompanyId)
                    return Forbid(); // User doesn't have access to the specified company

                // Remove the candidate from the database
                _dbContext.Candidates.Remove(candidate);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Candidate deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("GetCandidateById")]
        public IActionResult GetCandidateById(int candidateId)
        {
            try
            {
                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Retrieve the candidate with the specified CandidateId for the specific company
                var candidate = _dbContext.Candidates
                    .Where(c => c.FkCompanyId == loggedInCompanyId && c.CandidateId == candidateId)
                    .Select(c => new
                    {
                        CandidateId = c.CandidateId,
                        Fullname = c.Fullname,
                        Email = c.Email,
                        MobileNo = c.MobileNo,
                        Resume = c.Resume,
                        Skills = c.Skills,
                        ProfilePhoto = c.ProfilePhoto
                    })
                    .FirstOrDefault();

                if (candidate == null)
                {
                    return NotFound("Candidate not found.");
                }

                return Ok(candidate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPut("UpdateCandidate/{candidateId}")]
        public async Task<IActionResult> UpdateCandidate(int candidateId, [FromForm] CandidateUpdateModel candidateUpdateModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Retrieve the existing candidate
                var existingCandidate = _dbContext.Candidates
                    .FirstOrDefault(c => c.CandidateId == candidateId && c.FkCompanyId == loggedInCompanyId);

                if (existingCandidate == null)
                {
                    return NotFound("Candidate not found.");
                }

                // Update the candidate fields
                existingCandidate.Fullname = candidateUpdateModel.Fullname ?? existingCandidate.Fullname;
                existingCandidate.Email = candidateUpdateModel.Email ?? existingCandidate.Email;
                existingCandidate.MobileNo = candidateUpdateModel.MobileNo ?? existingCandidate.MobileNo;
                existingCandidate.Skills = candidateUpdateModel.Skills ?? existingCandidate.Skills;

                // Handle file updates (resume or profile photo)
                if (candidateUpdateModel.ResumeFile != null)
                {
                    // Handle resume file update
                    using (var resumeMemoryStream = new MemoryStream())
                    {
                        await candidateUpdateModel.ResumeFile.CopyToAsync(resumeMemoryStream);

                        // Update the resume file
                        var uniqueResumeFileName = Guid.NewGuid().ToString() + "_" + candidateUpdateModel.ResumeFile.FileName;
                        var resumeFilePath = Path.Combine("wwwroot", "images", uniqueResumeFileName);

                        using (var resumeFileStream = new FileStream(resumeFilePath, FileMode.Create))
                        {
                            resumeMemoryStream.Seek(0, SeekOrigin.Begin);
                            resumeMemoryStream.CopyTo(resumeFileStream);
                        }

                        existingCandidate.Resume = $"{BaseURL}/images/{uniqueResumeFileName}";
                    }
                }

                if (candidateUpdateModel.ProfilePhotoFile != null)
                {
                    // Handle profile photo file update
                    using (var photoMemoryStream = new MemoryStream())
                    {
                        await candidateUpdateModel.ProfilePhotoFile.CopyToAsync(photoMemoryStream);

                        // Update the profile photo file
                        var uniquePhotoFileName = Guid.NewGuid().ToString() + "_" + candidateUpdateModel.ProfilePhotoFile.FileName;
                        var photoFilePath = Path.Combine("wwwroot", "images", uniquePhotoFileName);

                        using (var photoFileStream = new FileStream(photoFilePath, FileMode.Create))
                        {
                            photoMemoryStream.Seek(0, SeekOrigin.Begin);
                            photoMemoryStream.CopyTo(photoFileStream);
                        }

                        existingCandidate.ProfilePhoto = $"{BaseURL}/images/{uniquePhotoFileName}";
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Ok("Candidate updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public class CandidateUpdateModel
        {
            public string? Fullname { get; set; }
            public string? Email { get; set; }
            public string? MobileNo { get; set; }
            public string? Skills { get; set; }
            public IFormFile? ResumeFile { get; set; }
            public IFormFile? ProfilePhotoFile { get; set; }
        }

        [HttpGet("GetEmailTemplateForStage")]
        public IActionResult GetEmailTemplateForStage(string stageName)
        {
            try
            {
                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Retrieve the associated email template for the specified stage
                var emailTemplate = _dbContext.EmailTemplates
                    .FirstOrDefault(et => et.Stage == stageName && et.FkCompanyId == loggedInCompanyId);

                if (emailTemplate == null)
                {
                    return NotFound("Email template not found for the specified stage.");
                }

                // Retrieve candidate information for the specified stage
                var candidateInfo = GetCandidateInfoForStage(loggedInCompanyId, stageName);

                if (candidateInfo == null)
                {
                    return NotFound("No candidate found for the specified stage.");
                }

                // Replace placeholders in the email template with actual values
                string replacedBody = ReplacePlaceholders(emailTemplate.Body, candidateInfo);

                return Ok(new
                {
                    Subject = emailTemplate.Subject,
                    Body = replacedBody,
                    CandidateEmail = candidateInfo.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private string ReplacePlaceholders(string template, CandidateInfoModel candidateInfo)
        {
            // Replace placeholders in the email template with actual values
            template = template.Replace("{Fullname}", candidateInfo.Fullname ?? "");
            template = template.Replace("{Email}", candidateInfo.Email ?? "");
            template = template.Replace("{CompanyName}", candidateInfo.CompanyName ?? "");

            // Log the replaced template
            Console.WriteLine($"Replaced Template: {template}");

            return template;
        }


        private CandidateInfoModel GetCandidateInfoForStage(int companyId, string stageName)
        {
            // Retrieve the candidate's information for the specified stage
            var candidateInfo = _dbContext.Candidates
                .Where(c => c.FkCompanyId == companyId && c.StagesName == stageName)
                .Select(c => new CandidateInfoModel
                {
                    Fullname = c.Fullname,
                    Email = c.Email,
                    // Use null-conditional operator outside of expression tree lambda
                    CompanyName = c.FkCompany != null ? c.FkCompany.CompanyName : null
                    // Add more properties as needed
                })
                .FirstOrDefault();
            Console.WriteLine("c.Fullname");

            return candidateInfo;
        }

        public class CandidateInfoModel
        {
            public string Fullname { get; set; }
            public string Email { get; set; }
            public string CompanyName { get; set; }
            // Add more properties as needed
        }
        [HttpPost("SendEmail")]
        public IActionResult SendEmail([FromBody] EmailRequestModel emailRequest)
        {
            try
            {
                if (emailRequest == null || string.IsNullOrEmpty(emailRequest.To) || string.IsNullOrEmpty(emailRequest.Subject) || string.IsNullOrEmpty(emailRequest.Body))
                {
                    return BadRequest("Invalid request data.");
                }

                // Retrieve user claims
                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == " email");
                var displayNameClaim = User.Claims.FirstOrDefault(c => c.Type == " email");

                if (emailClaim == null || displayNameClaim == null)
                {
                    return BadRequest("User claims not found.");
                }

                // Get email address and display name from claims
                var fromEmailAddress = emailClaim.Value;
                var fromDisplayName = displayNameClaim.Value;

                // NOTE: Replace the following code with your actual email sending logic
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new System.Net.NetworkCredential("dpatidar1221@gmail.com", "lveh yhaw szhz lydf");
                    smtpClient.EnableSsl = true;

                    using (var mailMessage = new MailMessage())
                    {
                        // Set the "From" address with display name
                        mailMessage.From = new MailAddress(fromEmailAddress, fromDisplayName);
                        mailMessage.To.Add(emailRequest.To);
                        mailMessage.Subject = emailRequest.Subject;
                        mailMessage.Body = emailRequest.Body;
                        mailMessage.IsBodyHtml = true;

                        // Set the "Reply-To" address
                        mailMessage.ReplyToList.Add(new MailAddress(fromEmailAddress, fromDisplayName));

                        smtpClient.Send(mailMessage);
                    }
                }

                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        public class EmailRequestModel
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        [HttpPost("import/candidates")]
        public IActionResult ImportCandidates(IFormFile file)
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
                    var candidates = ProcessCandidatesWorksheet(worksheet);

                    // Insert data into the database
                    InsertCandidatesIntoDatabase(candidates);

                    return Ok("Candidates import successful");
                }
            }
        }

        private List<CandidateDto> ProcessCandidatesWorksheet(ExcelWorksheet worksheet)
        {
            var candidates = new List<CandidateDto>();

            // Determine the last row with valid data
            int lastDataRow = worksheet.Dimension.Rows;
            for (int row = worksheet.Dimension.Rows; row >= 1; row--)
            {
                // Check if any key column has data (assuming columns 1, 2, 3, 4, 5, 6, 7, and 8 are key columns)
                if (!string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) ||
                    !string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) ||
                    !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) ||
                    !string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ||
                    !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ||
                    !string.IsNullOrEmpty(worksheet.Cells[row, 6].Text) ||
                    !string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ||
                    !string.IsNullOrEmpty(worksheet.Cells[row, 8].Text))
                {
                    lastDataRow = row;
                    break;
                }
            }

            // Process the data up to the last row with valid data
            for (int row = 2; row <= lastDataRow; row++)
            {
                var candidate = new CandidateDto
                {
                    Fullname = worksheet.Cells[row, 1].GetValue<string>(),
                    Email = worksheet.Cells[row, 2].GetValue<string>(),
                    MobileNo = worksheet.Cells[row, 3].GetValue<string>(),
                    Job = worksheet.Cells[row, 4].GetValue<string>(),
                    CreatedDate = worksheet.Cells[row, 5].GetValue<DateTime?>(),
                    CreatedBy = worksheet.Cells[row, 6].GetValue<string>(),
                    StagesName = worksheet.Cells[row, 7].GetValue<string>(),
                    JoinDate = worksheet.Cells[row, 8].GetValue<DateTime?>(),

                    // Map other properties accordingly
                };

                candidates.Add(candidate);
            }

            return candidates;
        }

        private void InsertCandidatesIntoDatabase(List<CandidateDto> candidates)
        {
            foreach (var candidateDto in candidates)
            {
                // Get CompanyId from token
                var companyId = GetCompanyIdFromToken();

                // Insert Job if not exists
                var job = _dbContext.Jobs
                    .FirstOrDefault(j => j.JobTittle == candidateDto.Job);

                if (job == null)
                {
                    job = new Job
                    {
                        JobTittle = candidateDto.Job,
                        FkCompanyId = companyId,
                        // Map other properties accordingly
                    };

                    _dbContext.Jobs.Add(job);
                    _dbContext.SaveChanges(); // Save changes to get the JobId
                }

                // Insert Candidate
                var newCandidate = new Candidate
                {
                    // Increment CandidateId
                    Fullname = candidateDto.Fullname,
                    Email = candidateDto.Email,
                    MobileNo = candidateDto.MobileNo,
                    FkJobId = job.JobId, // Link to the corresponding Job
                    CreatedDate = candidateDto.CreatedDate,
                    CreatedBy = candidateDto.CreatedBy,
                    StagesName = candidateDto.StagesName,
                    JoinDate = candidateDto.JoinDate,
                    FkCompanyId = companyId, // Set CompanyId here
                                             // Map other properties accordingly
                };

                _dbContext.Candidates.Add(newCandidate);
                _dbContext.SaveChanges();
            }
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

        private int GetNextCandidateId()
        {
            // Implement logic to get the next unique CandidateId
            // This could involve querying the database for the maximum existing CandidateId
            // and incrementing it.
            // For simplicity, you can use a counter, but in a real-world scenario, database-generated IDs are preferable.
            return _dbContext.Candidates.Max(c => (int?)c.CandidateId) + 1 ?? 1;
        }

        // Define a DTO class to hold the data from the XLS file
        public class CandidateDto
        {
            public string Fullname { get; set; }
            public string Email { get; set; }
            public string MobileNo { get; set; }
            public string Job { get; set; }
            public DateTime? CreatedDate { get; set; }
            public string CreatedBy { get; set; }
            public string StagesName { get; set; }
            public DateTime? JoinDate { get; set; }
            // Add other properties accordingly
        }

    }
}

