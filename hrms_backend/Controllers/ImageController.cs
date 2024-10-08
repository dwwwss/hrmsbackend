
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using hrms_backend.Models;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;
        private readonly string BaseURL; // Update this field to store the base URL

        public ImageController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;

            // Set the base URL here. Update this based on your deployment environment.
            BaseURL = "http://10.0.0.183/hrms";
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile image, int employeeId)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest("Invalid image file");
                }

                // Read the image into a byte array
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();

                    // Get the employee based on the provided employeeId
                    var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);

                    if (employee == null)
                    {
                        return NotFound("Employee not found");
                    }

                    // Generate a unique identifier for the image
                    var imageId = Guid.NewGuid().ToString();

                    // Save the image to a location (e.g., a directory) using the generated identifier
                    var imagePath = Path.Combine("wwwroot", "images", $"{imageId}.jpg");
                    System.IO.File.WriteAllBytes(imagePath, imageBytes);

                    // Update the employee object with the image URL
                    employee.Image = $"{BaseURL}/images/{imageId}.jpg";

                    // Save changes to the database
                    _dbContext.SaveChanges();

                    return Ok(new { Message = "Image uploaded successfully." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("{employeeId}/get-image")]
        public IActionResult GetEmployeeImage(int employeeId)
        {
            try
            {
                // Get the employee based on the provided employeeId
                var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);

                if (employee == null || string.IsNullOrEmpty(employee.Image))
                {
                    return NotFound("Employee image not found");
                }

                // Return the image URL
                return Ok(new { ImageUrl = employee.Image });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
