using hrms_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;
        private const string BaseURL = "http://10.0.0.183  /hrms";

        public ImportController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private void SaveResumeFile(IFormFile file, CandidateDto candidate)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;

                    var filePath = Path.Combine("wwwroot", "images", uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }

                    candidate.Resume = $"{BaseURL}/images/{uniqueFileName}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving resume file: {ex.Message}");
            }
        }

        [HttpPost("candidates")]
        public IActionResult ImportCandidates(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files were selected for upload");
            }

            var importedCandidates = new List<CandidateDto>();

            foreach (var file in files)
            {
                if (file == null || file.Length <= 0)
                {
                    continue;
                }

                using (var stream = file.OpenReadStream())
                {
                    if (IsPdfFile(file.FileName))
                    {
                        var pdfText = ExtractTextFromPdf(stream);
                        var candidateFromPdf = ParsePdfText(pdfText);
                        foreach (var candidate in candidateFromPdf)
                        {
                            SaveResumeFile(file, candidate);
                            importedCandidates.Add(candidate);
                        }
                    }
                    else
                    {
                        return BadRequest($"Unsupported file type: {file.FileName}");
                    }
                }
            }

            InsertCandidatesIntoDatabase(importedCandidates);

            return Ok("Candidates import successful");
        }

        private bool IsPdfFile(string fileName)
        {
            return Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private string ExtractTextFromPdf(Stream pdfStream)
        {
            using (var pdfReader = new iText.Kernel.Pdf.PdfReader(pdfStream))
            {
                var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);
                var text = new StringBuilder();

                for (int pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
                {
                    var page = pdfDocument.GetPage(pageNumber);
                    var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.LocationTextExtractionStrategy();
                    var currentText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page, strategy);

                    text.AppendLine(currentText);
                }
                Console.WriteLine(text);
                return text.ToString();
            }
        }
        private List<CandidateDto> ParsePdfText(string text)
        {
            var candidates = new List<CandidateDto>();
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            CandidateDto currentCandidate = null;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Skip empty lines
                    continue;
                }

                if (currentCandidate == null)
                {
                    currentCandidate = new CandidateDto();
                    candidates.Add(currentCandidate);

                    // Extract basic information from the line
                    ExtractInformationFromLine(line, currentCandidate);
                }
                else
                {
                    // Continue updating existing candidate information
                    UpdateCandidateInformation(line, currentCandidate);
                }
            }

            return candidates;
        }

        private void ExtractInformationFromLine(string line, CandidateDto candidate)
        {
            // Extract basic information such as fullname, email, and mobile number from the line
            candidate.Fullname = ExtractFullname(line);
            candidate.Email = ExtractEmail(line);
            candidate.MobileNo = ExtractMobileNumber(line);
        }

        private void UpdateCandidateInformation(string line, CandidateDto candidate)
        {
            // Update existing candidate information based on different conditions
            if (string.IsNullOrEmpty(candidate.Fullname))
            {
                candidate.Fullname = ExtractFullname(line);
            }
            else if (string.IsNullOrEmpty(candidate.Email))
            {
                candidate.Email = ExtractEmail(line);
            }
            else if (string.IsNullOrEmpty(candidate.MobileNo))
            {
                candidate.MobileNo = ExtractMobileNumber(line);
            }
        }

        // Your existing ExtractEmail, ExtractMobileNumber, and ExtractFullname methods remain the same.



        private string ExtractMobileNumber(string line)
        {
            var match = Regex.Match(line, @"\b(?:Mobile|Contact|Contact No\.?|Mobile No\.?|Mobile No\.? :-|Contact -)\s*[:\-]?\s*\+?(\d{10,12})\b");

            return match.Success ? match.Groups[1].Value : "";
        }


        private string ExtractEmail(string line)
        {
            var match = Regex.Match(line, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");
            return match.Success ? match.Value : "";
        }

        private string ExtractFullname(string line)
        {
            // Trim the line and check if it's not empty
            var trimmedLine = line.Trim();
            if (!string.IsNullOrEmpty(trimmedLine))
            {
                return trimmedLine;
            }

            // If the line is empty, return an empty string
            return "";
        }


        private void InsertCandidatesIntoDatabase(List<CandidateDto> candidates)
        {
            try
            {
                var companyId = GetCompanyIdFromToken();
                var employeeId = GetEmployeeIdFromToken();
                var employeeName = GetEmployeeNameById(employeeId);


                foreach (var candidateDto in candidates)
                {
                    if (candidateDto != null)
                    {
                        // Log the parsed data for debugging
                        Console.WriteLine($"Fullname: {candidateDto.Fullname}, Email: {candidateDto.Email}, MobileNo: {candidateDto.MobileNo}, Resume: {candidateDto.Resume}");

                        _dbContext.Candidates.Add(new Candidate
                        {
                            Fullname = candidateDto.Fullname,
                            Email = candidateDto.Email,
                            MobileNo = candidateDto.MobileNo,
                            Resume = candidateDto.Resume,
                            FkCompanyId = companyId,
                            StagesName ="Apllied",
                            CreatedDate=DateTime.Now,
                            CreatedBy = employeeName,
                            // ... Set other properties
                        });
                    }
                }

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // Handle database insertion error
                Console.WriteLine($"Error inserting candidates into the database: {ex.Message}");
            }
        }
        // Example implementation for retrieving employee ID from JWT token
        private int GetEmployeeIdFromToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var claim = identity.FindFirst("employeeId");
                if (claim != null && int.TryParse(claim.Value, out int employeeId))
                {
                    return employeeId;
                }
            }

            // Return a default value or handle the absence of a valid employee ID in the token
            return 0;
        }

        // Example implementation for retrieving employee name by ID
        private string GetEmployeeNameById(int employeeId)
        {
            // Assuming you have an Employee table in your database
            var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);

            return employee?.FullName ?? "Unknown";
        }

        private int GetCompanyIdFromToken()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            var employeeIdClaim = User.Claims.FirstOrDefault(i => i.Type == "EmployeeId");
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                throw new InvalidOperationException("Invalid data. Company information is missing.");
            }

            return companyId;
        }
    

        public class CandidateDto
        {
            public string Fullname { get; set; }
            public string Email { get; set; }
            public string MobileNo { get; set; }
            public string Resume { get; set; }
        }
        [HttpPut("{candidateId}/AssignJob/{jobId}")]
        public IActionResult AssignJob(int candidateId, int jobId)
        {
            try
            {
                var candidate = _dbContext.Candidates.FirstOrDefault(c => c.CandidateId == candidateId);

                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {candidateId} not found.");
                }

                var job = _dbContext.Jobs.FirstOrDefault(j => j.JobId == jobId);

                if (job == null)
                {
                    return NotFound($"Job with ID {jobId} not found.");
                }

                candidate.FkJobId = jobId;

                // Save changes to the database
                _dbContext.SaveChanges();

                return Ok($"Job  assigned to Candidate successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception or return an appropriate error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while assigning job to the candidate with ID {candidateId}.");
            }
        }
    }
}
