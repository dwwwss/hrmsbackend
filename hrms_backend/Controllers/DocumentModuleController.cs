using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using hrms_backend.Models; // Make sure to include the namespace where your Document model is defined
using Microsoft.EntityFrameworkCore;

namespace hrms_backend.Controllers
{
    [Route("api/documents")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly HrmsDbContext _dbContext; // Inject hrmsdbcontext

        public DocumentController(IWebHostEnvironment webHostEnvironment, HrmsDbContext dbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
        }

        [HttpPost("upload/{employeeId}")]
        public async Task<IActionResult> UploadDocument(int employeeId)
        {
            try
            {
                var file = Request.Form.Files[0]; // Assuming you're using form-data for file upload

                if (file != null && file.Length > 0)
                {
                    // Fetch employee information from the database based on employeeId
                    var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);

                    if (employee != null)
                    {
                        // Save document details in the database
                        var document = new Document
                        {
                            FkEmployeeId = employeeId,
                            DocumentType = "1",
                            DocumentName = file.FileName, // Use the original file name as the document name
                            UploadedDate = DateTime.UtcNow,
                            UploadedBy = employee.FullName, // Use the employee name from the database
                            IsActive = true
                        };

                        // Save to the database using Entity Framework
                        _dbContext.Documents.Add(document);
                        _dbContext.SaveChanges();

                        // Use the DocumentId after saving to the database
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Use a unique filename
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Employee", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Update the FilePath property and save changes to the database
                        document.FilePath = filePath;
                        _dbContext.SaveChanges();
                        // Retrieve the maximum EmployeeDocumentId from the database
                        var maxEmployeeDocumentId = _dbContext.EmployeeDocuments.Max(ed => ed.EmployeeDocumentId);

                        // Increment the retrieved value by one
                        var nextEmployeeDocumentId = maxEmployeeDocumentId + 1;
                        // Automatically insert a record into EmployeeDocument table
                        var employeeDocument = new EmployeeDocument
                        {

                            EmployeeDocumentId = nextEmployeeDocumentId, // Set your desired ID manually
                            FkEmployeeId = employeeId,
                            FkDocumentId = document.DocumentId,
                            IsActive = true
                           
                        };

                        _dbContext.EmployeeDocuments.Add(employeeDocument);
                        _dbContext.SaveChanges();

                        return Ok($"File uploaded successfully to {filePath}");
                    }
                    else
                    {
                        return NotFound($"Employee with ID {employeeId} not found");
                    }
                }

                return BadRequest("No file uploaded");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Define a model for the request body
        public class DocumentRequestModel
        {
            public string DocumentName { get; set; }
        }


        [HttpGet("employee/{documentId}")]
        public IActionResult GetDocument(int documentId)
        {
            try
            {
                // Check if the request is coming from a browser
                string userAgent = Request.Headers["User-Agent"].ToString();
                bool isBrowserRequest = !string.IsNullOrEmpty(userAgent) && userAgent.Contains("Mozilla", StringComparison.OrdinalIgnoreCase);

                if (isBrowserRequest)
                {
                    // If the request is from a browser, return Access Denied
                    return StatusCode(403, "Access Denied");
                }

                // Retrieve document details from the database based on documentId
                var document = _dbContext.Documents.FirstOrDefault(d => d.DocumentId == documentId);

                if (document == null)
                {
                    return NotFound($"Document with ID {documentId} not found");
                }

                // Check if the file exists
                if (!System.IO.File.Exists(document.FilePath))
                {
                    return NotFound($"File not found for Document ID {documentId}");
                }

                // Read the file content and return it as the response
                var fileContent = System.IO.File.ReadAllBytes(document.FilePath);

                return File(fileContent, "application/octet-stream", Path.GetFileName(document.FilePath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("employee/{employeeId}/documents")]
        public IActionResult GetEmployeeDocuments(int employeeId)
        {
            try
            {
                // Retrieve documents for the specified employeeId
                var employeeDocuments = _dbContext.EmployeeDocuments
                    .Where(ed => ed.FkEmployeeId == employeeId)
                    .Include(ed => ed.FkDocument) // Include the related Document
                    .ToList();

                if (employeeDocuments.Count == 0)
                {
                    return NotFound($"No documents found for Employee ID {employeeId}");
                }

                // Map the results to a DTO (Data Transfer Object) for response
                var result = employeeDocuments.Select(ed => new
                {
                    DocumentId = ed.FkDocumentId,
                    DocumentName = ed.FkDocument?.DocumentName,
                    UploadedDate = ed.FkDocument?.UploadedDate,
                    UploadedBy = ed.FkDocument?.UploadedBy,
                    DownloadLink = Url.Action("GetDocument", "Document", new { documentId = ed.FkDocumentId })
                }).ToList();


                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("document/{documentId}")]
        public IActionResult DeleteDocument(int documentId)
        {
            try
            {
                // Retrieve document details from the database based on documentId
                var document = _dbContext.Documents.FirstOrDefault(d => d.DocumentId == documentId);

                if (document == null)
                {
                    return NotFound($"Document with ID {documentId} not found");
                }

                // Check if the file exists
                // Assuming documentId is the ID of the document you want to delete
                var relatedEmployeeDocuments = _dbContext.EmployeeDocuments.Where(ed => ed.FkDocumentId == documentId);
                _dbContext.EmployeeDocuments.RemoveRange(relatedEmployeeDocuments);

                var documentToDelete = _dbContext.Documents.FirstOrDefault(d => d.DocumentId == documentId);
                if (documentToDelete != null)
                {
                    _dbContext.Documents.Remove(documentToDelete);
                    _dbContext.SaveChanges();
                }


                return Ok($"Document with ID {documentId} deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
