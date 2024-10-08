using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using hrms_backend.Models;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;

        public SettingController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet("GetHolidays")]
        public ActionResult<IEnumerable<HolidayDto>> GetHolidays()
        {
            // Retrieve company ID from claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return BadRequest("Invalid or missing Company ID claim");
            }

            var holidays = _dbContext.Holidays
                .Where(h => h.FkCompanyId == companyId)
                .Select(h => new HolidayDto
                {
                    HolidayId = h.HolidayId,
                    HolidayName = h.HolidayName,
                    FromDate = h.FromDate.HasValue ? h.FromDate.Value.ToString("d MMM yyyy") : null,
                    ToDate = h.ToDate.HasValue ? h.ToDate.Value.ToString("d MMM yyyy") : null
                })
                .ToList();

            return Ok(holidays);
        }

        [HttpPost("AddHoliday")]
        public ActionResult<HolidayDto> AddHoliday([FromBody] HolidayInputModel holidayInput)
        {
            // Retrieve company ID from claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return BadRequest("Invalid or missing Company ID claim");
            }

            if (holidayInput == null || string.IsNullOrWhiteSpace(holidayInput.HolidayName) ||
                holidayInput.FromDate == null || holidayInput.ToDate == null)
            {
                return BadRequest("Invalid holiday data");
            }

            var holiday = new Holiday
            {
                HolidayName = holidayInput.HolidayName,
                FromDate = holidayInput.FromDate,
                ToDate = holidayInput.ToDate,
                FkCompanyId = companyId
            };

            _dbContext.Holidays.Add(holiday);
            _dbContext.SaveChanges();

            var holidayDto = new HolidayDto
            {
                HolidayName = holiday.HolidayName,
                FromDate = holiday.FromDate?.ToString("yyyy-MM-dd"),
                ToDate = holiday.ToDate?.ToString("yyyy-MM-dd")
            };

            return CreatedAtAction(nameof(GetHolidays), holidayDto);
        }

        public class HolidayInputModel
        {
            public string HolidayName { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }

        public class HolidayDto
        {
            public int HolidayId { get; set; }
            public string HolidayName { get; set; }
            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
        }

        [HttpDelete("DeleteHoliday/{HolidayId}")]
        public ActionResult DeleteComment(int HolidayId)
        {
            var Holiday = _dbContext.Holidays.Find(HolidayId);

            if (Holiday == null)
            {
                return NotFound("Holiday not found");
            }

            _dbContext.Holidays.Remove(Holiday);
            _dbContext.SaveChanges();

            return Ok("Holiday deleted successfully");
        }

    }
}
