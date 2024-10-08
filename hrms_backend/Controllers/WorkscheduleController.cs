using hrms_backend.Models;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkscheduleController : Controller
    {
        private readonly HrmsDbContext _dbContext;
        public WorkscheduleController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        //Get Work Schedule --
        [HttpGet("WorkSchedule")]
        public IActionResult GetAllWorkschedules()
        {
            // Retrieve all Workschedules from the database
            var allWorkschedules = _dbContext.Workschedules.ToList();
            return Ok(allWorkschedules); // Return all Workschedules
        }
        //Post WorkSchdeule API
        [HttpPost("create")]
        public IActionResult CreateWorkschedule([FromBody] Workschedule workschedule)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Deserialize the workingDays JSON string into a dictionary
            var workingDaysDict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(workschedule.WorkingDays);
            // Serialize the dictionary back to a JSON string
            workschedule.WorkingDays = JsonConvert.SerializeObject(workingDaysDict);
            // Set CreatedDate and ModifiedDate
            workschedule.CreatedDate = DateTime.Now;
            workschedule.ModifiedDate = workschedule.CreatedDate;
            // Add workschedule to the database
            _dbContext.Workschedules.Add(workschedule);
            _dbContext.SaveChanges();
            return Ok(workschedule); // Return the created workschedule
        }
        // PUT API for Workschedule
        [HttpPut("WorkSchedule/{id}")]
        public IActionResult UpdateWorkSchedule(int id, [FromBody] Workschedule updatedScheduleDto)
        {
            if (updatedScheduleDto == null)
            {
                return BadRequest("Invalid data provided");
            }
            var existingSchedule = _dbContext.Workschedules.FirstOrDefault(ws => ws.ScheduleId == id);
            if (existingSchedule == null)
            {
                return NotFound();
            }
            try
            {
                existingSchedule.ScheduleName = updatedScheduleDto.ScheduleName;
                existingSchedule.Description = updatedScheduleDto.Description;
                existingSchedule.HoursPerDay = updatedScheduleDto.HoursPerDay;
                existingSchedule.ScheduleType = updatedScheduleDto.ScheduleType;
                existingSchedule.HoursPerWeek = updatedScheduleDto.HoursPerWeek;
                existingSchedule.FkCompanyId = updatedScheduleDto.FkCompanyId;
                existingSchedule.DailyWorkingHours = updatedScheduleDto.DailyWorkingHours;
                existingSchedule.WorkingDays = updatedScheduleDto.WorkingDays; // Update WorkingDays directly
                existingSchedule.CreatedDate = updatedScheduleDto.CreatedDate;
                existingSchedule.CreatedBy = updatedScheduleDto.CreatedBy;
                existingSchedule.ModifiedDate = updatedScheduleDto.ModifiedDate;
                existingSchedule.ModifiedBy = updatedScheduleDto.ModifiedBy;
                existingSchedule.IsActive = updatedScheduleDto.IsActive;
                existingSchedule.StartTime = updatedScheduleDto.StartTime;
                existingSchedule.EndTime = updatedScheduleDto.EndTime;
                existingSchedule.LateTime = updatedScheduleDto.LateTime;
                existingSchedule.HalfDayTime = updatedScheduleDto.HalfDayTime;
                _dbContext.SaveChanges();
                return Ok("Work schedule updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating work schedule: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        //Delete WorkSchedule
        [HttpDelete("WorkSchedule/{id}")]
        public IActionResult DeleteWorkSchedule(int id)
        {
            try
            {
                var existingSchedule = _dbContext.Workschedules.FirstOrDefault(ws => ws.ScheduleId == id);
                if (existingSchedule == null)
                {
                    return NotFound();
                }
                _dbContext.Workschedules.Remove(existingSchedule);
                _dbContext.SaveChanges();
                return Ok("Work schedule deleted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting work schedule: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        // GET: api/Workschedule/GetWorkScheduleByEmployeeId/{employeeId}
        [HttpGet("ByEmployeeId/{employeeId}")]
        public async Task<ActionResult<Workschedule>> GetWorkScheduleByEmployeeId(int employeeId)
        {
            try
            {
                // Find the employee by ID including the related schedule
                var employee = await _dbContext.Employees
                    .Include(e => e.FkSchedule)
                    .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {employeeId} not found.");
                }
                // Return the schedule information
                return Ok(employee.FkSchedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        // POST: api/Employee/AssignSchedule
        [HttpPost("Assignschedule")]
        public async Task<ActionResult> AssignScheduleToEmployee([FromBody] WorkscheduleEmp request)
        {
            try
            {
                // Find the employee by ID
                var employee = await _dbContext.Employees.FindAsync(request.EmployeeId);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {request.EmployeeId} not found.");
                }
                // Find the schedule by ID
                var schedule = await _dbContext.Workschedules.FindAsync(request.ScheduleId);
                if (schedule == null)
                {
                    return NotFound($"Work schedule with ID {request.ScheduleId} not found.");
                }
                // Assign the schedule to the employee
                employee.FkScheduleId = request.ScheduleId;
                // Save changes to the database
                await _dbContext.SaveChangesAsync();
                return Ok($"Work schedule assigned to employee with ID {request.EmployeeId} successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public class WorkscheduleEmp
        {
            public int EmployeeId { get; set; } // Assuming EmployeeId is an integer
            public int ScheduleId { get; set; } // Assuming ScheduleId is an integer
        }
    }
}