using hrms_backend.Models;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficeController : Controller
    {
        private readonly HrmsDbContext _dbContext;
        public OfficeController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult GetOfficeByEmployeeId(int employeeId)
        {
            var employee = _dbContext.Employees
                .Include(e => e.FkOffice) // Include the related Office entity
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (employee == null || employee.FkOffice == null)
            {
                return NotFound($"Employee with ID {employeeId} not found or office information not available");
            }

            var officeDto = new Office
            {
                OfficeId = employee.FkOffice.OfficeId,
                OfficeName = employee.FkOffice.OfficeName,
                Address = employee.FkOffice.Address,
                City = employee.FkOffice.City,
                State = employee.FkOffice.State,
                Country = employee.FkOffice.Country,
                PostalCode = employee.FkOffice.PostalCode,
                CreatedDate = employee.FkOffice.CreatedDate,
                CreatedBy = employee.FkOffice.CreatedBy,
                ModifiedDate = employee.FkOffice.ModifiedDate,
                ModifiedBy = employee.FkOffice.ModifiedBy,
                IsActive = employee.FkOffice.IsActive,
                FkCompanyId = employee.FkOffice.FkCompanyId,
                FkScheduleId = employee.FkOffice.FkScheduleId,
                ContactNo = employee.FkOffice.ContactNo,
                Email = employee.FkOffice.Email,
                Latitude = employee.FkOffice.Latitude,
                Longitude = employee.FkOffice.Longitude,
                Qrcode = employee.FkOffice.Qrcode,
                Radius = employee.FkOffice.Radius
            };

            return Ok(officeDto);
        }
        [HttpPost("OfficeAssign")]
        public IActionResult AssignOfficeToEmployee(int employeeId, int officeId)
        {
            var employee = _dbContext.Employees.Find(employeeId);
            var office = _dbContext.Offices.Find(officeId);

            if (employee == null)
            {
                return NotFound($"Employee with ID {employeeId} not found");
            }

            if (office == null)
            {
                return NotFound($"Office with ID {officeId} not found");
            }

            // Assign the office to the employee
            employee.FkOfficeId = officeId;

            try
            {
                _dbContext.SaveChanges();
                return Ok($"Office with ID {officeId} assigned to Employee with ID {employeeId}");
            }
            catch (Exception ex)
            {
                // Handle any exceptions, e.g., database update failure
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Alloffice")]
        public IActionResult GetOffices()
        {
            // Get the company ID from the user's token
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
            {
                return BadRequest("Invalid data. Company information is missing.");
            }

            // Filter offices based on the company ID
            var offices = _dbContext.Offices
                .AsNoTracking()
                .Where(o => o.FkCompanyId == loggedInCompanyId)
                .ToList();

            var officeDtos = offices.Select(o => new Office
            {
                OfficeId = o.OfficeId,
                OfficeName = o.OfficeName,
                Address = o.Address,
                City = o.City,
                State = o.State,
                Country = o.Country,
                PostalCode = o.PostalCode,
                CreatedDate = o.CreatedDate,
                CreatedBy = o.CreatedBy,
                ModifiedDate = o.ModifiedDate,
                ModifiedBy = o.ModifiedBy,
                IsActive = o.IsActive,
                FkCompanyId = o.FkCompanyId,
                FkScheduleId = o.FkScheduleId,
                ContactNo = o.ContactNo,
                Email = o.Email,
                Latitude = o.Latitude,
                Longitude = o.Longitude,
                Qrcode = o.Qrcode,
                Radius = o.Radius
            }).ToList();

            return Ok(officeDtos);
        }

        [HttpPost]
        public IActionResult CreateOffice([FromBody] Office office)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the company ID from the user's token
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
            {
                return BadRequest("Invalid data. Company information is missing.");
            }

            // Set the FkCompanyId property of the office entity
            office.FkCompanyId = loggedInCompanyId;

            // Save the office entity
            _dbContext.Offices.Add(office);
            _dbContext.SaveChanges();

            return Ok(office);
        }

        //Update API
        [HttpPut("{officeId}")]
        public IActionResult UpdateOffice(int officeId, [FromBody] Office updatedOffice)
        {
            var office = _dbContext.Offices.FirstOrDefault(o => o.OfficeId == officeId);
            if (office == null)
            {
                return NotFound(); // Office with the given ID not found
            }
            office.OfficeName = updatedOffice.OfficeName;
            office.Address = updatedOffice.Address;
            office.Latitude = updatedOffice.Latitude;
            office.Longitude = updatedOffice.Longitude;
            // Update country, state, and city IDs
            office.FkCountryId = updatedOffice.FkCountryId;
            office.FkStateId = updatedOffice.FkStateId;
            office.FkCityId = updatedOffice.FkCityId;
            _dbContext.SaveChanges();
            return Ok(new { message = "Office successfully updated" }); // Success message
        }
        //Delete API
        [HttpDelete("{officeId}")]
        public IActionResult DeleteOffice(int officeId)
        {
            var office = _dbContext.Offices
                .Include(o => o.Employees) // Include associated employees
                .FirstOrDefault(o => o.OfficeId == officeId);

            if (office == null)
            {
                return NotFound(); // Office with the given ID not found
            }

            // Set FkOfficeId to null for associated employees
            foreach (var employee in office.Employees)
            {
                employee.FkOfficeId = null;
            }

            // Now remove the office
            _dbContext.Offices.Remove(office);

            // Save changes
            _dbContext.SaveChanges();

            return Ok(new { message = "Office successfully deleted" }); // Success message
        }
        [HttpGet("getNextOfficeId")]
        public IActionResult GetNextOfficeId()
        {
            try
            {
                // Get the highest existing OfficeId
                var maxOfficeId = _dbContext.Offices.Max(o => (int?)o.OfficeId) ?? 0;

                // Increment the highest existing OfficeId to get the next available OfficeId
                var nextOfficeId = maxOfficeId + 1;

                return Ok(new { NextOfficeId = nextOfficeId });
            }
            catch (Exception ex)
            {
                // Log the exception details or return an error response
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching the next OfficeId.");
            }
        }

    }
}