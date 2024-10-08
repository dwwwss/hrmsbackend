using hrms_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployementTypeController : Controller
    {
        private readonly HrmsDbContext _dbContext;
        public EmployementTypeController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        // GET: api/EmploymentType
        [HttpGet]
        public IEnumerable<EmployementType> GetEmploymentTypes()
        {
            return _dbContext.EmployementTypes;
        }

        [HttpPost("AssignToEmployee")]
        public IActionResult AssignEmploymentTypeToEmployee([FromBody] EmploymentAssignmentModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == model.EmployeeId);

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            employee.FkEmployementTypeId = model.EmploymentTypeId;
            _dbContext.SaveChanges();

            return Ok("Employment type assigned successfully");
        }
       
        public class EmploymentAssignmentModel
        {
            public int EmployeeId { get; set; }
            public int EmploymentTypeId { get; set; }
        }
        [HttpGet("EmployementType/{employeeId}")]
        public IActionResult GetEmploymentTypeByEmployeeId(int employeeId)
        {
            var employee = _dbContext.Employees
                .Include(e => e.FkEmployementType) // Include employment type navigation property
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            var employmentType = employee.FkEmployementType;

            if (employmentType == null)
            {
                return NotFound("Employment type not assigned to the employee");
            }

            var employmentTypeDetails = new
            {
                EmployeeId = employee.EmployeeId,
                EmploymentTypeId = employmentType.EmployeeTypeId,
                TypeName = employmentType.TypeName,
                // Include other properties as needed
            };

            return Ok(employmentTypeDetails);
        }
    }
}