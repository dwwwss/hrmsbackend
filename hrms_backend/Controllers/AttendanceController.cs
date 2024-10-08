using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using hrms_backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace hrms_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext; 

        public AttendanceController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("loggedsessions/{employeeId}")]
        public IActionResult GetLoggedSessions(int employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var employee = _dbContext.Employees.Find(employeeId);

                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                // Extract timezone from headers
                var timeZoneInfo = GetTimeZoneInfoFromRequestHeaders(HttpContext.Request.Headers);

                // Convert start and end dates to UTC
                var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDate, timeZoneInfo);
                var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(endDate, timeZoneInfo);

                var loggedSessions = _dbContext.AttendanceSessions
                    .Where(a => a.FkEmployeeId == employeeId &&
                                 a.ClockinTime.HasValue &&
                                 ((a.ClockinTime >= startDateUtc || a.ClockoutTime >= startDateUtc) &&
                                  (a.ClockinTime < endDateUtc.AddDays(1) || a.ClockoutTime < endDateUtc.AddDays(1))))
                    .AsEnumerable()
                    .GroupBy(a => a.ClockinTime?.Date ?? DateTime.MinValue, new DateTimeComparer())
                    .Select(group => new
                    {
                        Date = TimeZoneInfo.ConvertTimeFromUtc(group.Key, timeZoneInfo), // Convert the group key (date) back to the desired timezone
                        TotalLoggedTime = group.Sum(a => a.Type == false ? (a.ClockoutTime - a.ClockinTime)?.TotalHours ?? 0 : 0),
                        Sessions = GetLoggedSessionDetails(group.ToList(), timeZoneInfo)
                    })
                    .ToList();

                return Ok(loggedSessions);
            }
            catch (Exception ex)
            {
               
                // Log.Error(ex, "An error occurred while processing the request.");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
        private List<object> GetLoggedSessionDetails(List<AttendanceSession> sessions, TimeZoneInfo timeZoneInfo)
        {
            return sessions
                .OrderBy(a => a.ClockinTime)
                .Select(a => (object)new
                {
                    EventInTime = TimeZoneInfo.ConvertTimeFromUtc(a.ClockinTime ?? DateTime.MinValue, timeZoneInfo),
                    EventOutTime = a.Type == true ? (DateTime?)null : TimeZoneInfo.ConvertTimeFromUtc(a.ClockoutTime ?? DateTime.MinValue, timeZoneInfo),
                    LoggedTime = a.Type == false ? ConvertToHoursAndMinutes(a.ClockoutTime - a.ClockinTime) : (string)null,
                    Type = (bool)a.Type ? 1 : 0,
                    IsClockInGeofence = a.Clockingeofence,
                    IsClockOutGeofence = a.Type == true ? (bool?)null : a.ClockOutGeofence,
                })
                .ToList<object>();
        }

        private string ConvertToHoursAndMinutes(TimeSpan? timeSpan)
        {
            if (timeSpan.HasValue)
            {
                return $"{(int)timeSpan.Value.TotalHours} hr {timeSpan.Value.Minutes} min";
            }
            return string.Empty;
        }

        private bool IsLocationWithinRadius(double latitude, double longitude, double headquartersLatitude, double headquartersLongitude, double radiusMeters)
        {
            // Convert latitude and longitude differences to meters
            var latitudeDifference = (latitude - headquartersLatitude) * 111000; // 1 degree latitude is approximately 111,000 meters
            var longitudeDifference = (longitude - headquartersLongitude) * 111000 * Math.Cos(headquartersLatitude * Math.PI / 180);

            // Calculate the distance
            var distance = Math.Sqrt(Math.Pow(latitudeDifference, 2) + Math.Pow(longitudeDifference, 2));

            // Check if the distance is within the specified radius
            return distance <= radiusMeters;
        }

        private TimeZoneInfo GetTimeZoneInfoFromRequestHeaders(IHeaderDictionary headers)
        {
            const string timeZoneHeaderKey = "TimeZoneId";
            string timeZoneId;

            // Check if the header is present in the request
            if (headers.TryGetValue(timeZoneHeaderKey, out var timeZoneValues))
            {
                timeZoneId = timeZoneValues.FirstOrDefault();
            }
            else
            {
             
                // For example, return UTC as the default time zone:
                return TimeZoneInfo.Utc;
            }

            try
            {
                // Attempt to find the time zone by the provided ID
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return timeZoneInfo;
            }
            catch (TimeZoneNotFoundException)
            {
             
                return TimeZoneInfo.Utc;
            }
            catch (InvalidTimeZoneException)
            {

                return TimeZoneInfo.Utc;
            }
        }
        //attendancesession 

        [HttpPost("clockin/{employeeId}")]
        public IActionResult ClockIn(int employeeId, [FromBody] AttendanceClockInRequest request)
        {
            var currentTimeUtc = DateTime.UtcNow;
            var timeZoneInfo = GetTimeZoneInfoFromRequestHeaders(HttpContext.Request.Headers);

            var currentTimeWithTimeZone = TimeZoneInfo.ConvertTimeFromUtc(currentTimeUtc, timeZoneInfo);

            var employee = _dbContext.Employees.Find(employeeId);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

         
            // Check if the employee already has an active clock-in session
            if (employee.CurrentSessionId.HasValue)
            {
                // If the current session is for the previous day in the specified time zone, automatically clock out the employee
                var activeSession = _dbContext.AttendanceSessions.FirstOrDefault(a => a.AtttendnaceSessionId == employee.CurrentSessionId.Value);
                if (activeSession != null &&
                    TimeZoneInfo.ConvertTimeFromUtc(activeSession.ClockinTime ?? DateTime.UtcNow, timeZoneInfo).Date < currentTimeWithTimeZone.Date)
                {
                    activeSession.Type = null; // Clock out the employee
                    activeSession.ClockoutTime = null; // Set the clock-out time
                    _dbContext.SaveChanges();

                    // Clear the current session ID for the employee
                    employee.CurrentSessionId = null;
                    _dbContext.SaveChanges();

                    // Temporary debug statement
                    Console.WriteLine($"Session expired successfully in {timeZoneInfo.DisplayName} time zone.");
                }
                else
                {
                    return BadRequest("The employee has already clocked in. Clock-out first before clocking in.");
                }
            }

            // Fetch office details based on FkOfficeId from the request
            var office = _dbContext.Offices.FirstOrDefault(o => o.OfficeId == request.FkOfficeId);

            if (office == null)
            {
                return BadRequest("Invalid FkOfficeId.");
            }

            // Check if the clock-in location is within the specified radius of the headquarters
            var isClockInAtHeadquarters = IsLocationWithinRadius(
                Convert.ToDouble(request.Latitude),
                Convert.ToDouble(request.Longitude),
                Convert.ToDouble(office.Latitude),
                Convert.ToDouble(office.Longitude),
                Convert.ToDouble(office.Radius));

            
            // Create a new attendance session record for clock-in
            var clockInSession = new AttendanceSession
            {
                FkEmployeeId = employeeId,
                FkOfficeId = request.FkOfficeId,
                Type = true, // Indicate clock-in
                ClockInLatitude = (decimal)Convert.ToDouble(request.Latitude),
                ClockInLongitude = (decimal)Convert.ToDouble(request.Longitude),
                ClockinTime = currentTimeUtc, // Store in UTC
                Clockingeofence = isClockInAtHeadquarters // 1 for inside geofence, 0 for outside
            };

            _dbContext.AttendanceSessions.Add(clockInSession);
            _dbContext.SaveChanges();


            // Update the current session ID for the employee
            employee.CurrentSessionId = clockInSession.AtttendnaceSessionId;
            _dbContext.SaveChanges();

            // Determine if the clock-in happened at the headquarters or outside
            var clockInLocation = isClockInAtHeadquarters ? "Headquarters" : "Outside";

            return Ok($"Clock-in successful. Clock-in location: {clockInLocation}");
        }
        [HttpPost("clockout/{employeeId}")]
        public IActionResult ClockOut(int employeeId, [FromBody] AttendanceClockOutRequest request)
        {
            try
            {
                var currentTimeUtc = DateTime.UtcNow;
                var timeZoneInfo = GetTimeZoneInfoFromRequestHeaders(HttpContext.Request.Headers);
                var currentTimeWithTimeZone = TimeZoneInfo.ConvertTimeFromUtc(currentTimeUtc, timeZoneInfo);

                var employee = _dbContext.Employees.Find(employeeId);

                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                // Check if the employee has an active clock-in session
                if (employee.CurrentSessionId.HasValue)
                {
                    // Retrieve the clock-in attendance session record
                    var clockInSession = _dbContext.AttendanceSessions.Find(employee.CurrentSessionId.Value);

                    // Fetch office details based on FkOfficeId from the clock-in session
                    var office = clockInSession != null ? _dbContext.Offices.FirstOrDefault(o => o.OfficeId == clockInSession.FkOfficeId) : null;

                    // Update the existing AttendanceSession record for clock-out
                    if (clockInSession != null)
                    {
                        clockInSession.Type = false; // Indicate clock-out
                        clockInSession.ClockOutLatitude = Convert.ToDecimal(request.Latitude);
                        clockInSession.ClockOutLongitude = Convert.ToDecimal(request.Longitude);
                        clockInSession.ClockoutTime = currentTimeUtc;

                        // Calculate clockoutgeofence
                        var isClockOutAtHeadquarters = IsLocationWithinRadius(
                            Convert.ToDouble(request.Latitude),
                            Convert.ToDouble(request.Longitude),
                            Convert.ToDouble(office?.Latitude ?? 0),
                            Convert.ToDouble(office?.Longitude ?? 0),
                            Convert.ToDouble(office?.Radius ?? 0));

                        clockInSession.ClockOutGeofence = isClockOutAtHeadquarters;

                        _dbContext.SaveChanges();
                    }

                    // Clear the current session ID for the employee
                    employee.CurrentSessionId = null;
                    _dbContext.SaveChanges();

                    // Determine if the clock-out happened at the headquarters or outside
                    var clockOutLocation = clockInSession?.ClockOutGeofence == true ? "Headquarters" : "Outside";

                    return Ok($"Clock-out successful. Clock-out location: {clockOutLocation}");
                }

                // If the employee does not have an active session, return a response indicating the same.
                return BadRequest("The employee does not have an active session to clock out.");
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        [HttpGet("logged/{employeeId}")]
        public IActionResult GetLoggedTime(int employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var employee = _dbContext.Employees.Find(employeeId);
                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }
                // Extract timezone from headers
                var timeZoneInfo = GetTimeZoneInfoFromRequestHeaders(HttpContext.Request.Headers);
                // Convert start date to UTC
                var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDate, timeZoneInfo);

                // Adjust end date to 23:59:59 and then convert to UTC
                var adjustedEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);
                var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(adjustedEndDate, timeZoneInfo).AddDays(1);
                var loggedSessions = _dbContext.AttendanceSessions
                    .Where(a => a.FkEmployeeId == employeeId &&
                                a.ClockinTime.HasValue &&
                                a.Type.HasValue &&
                                ((a.ClockinTime >= startDateUtc || a.ClockoutTime >= startDateUtc) &&
                                (a.ClockinTime < endDateUtc || a.ClockoutTime < endDateUtc)))
                      .AsEnumerable()
      .GroupBy(a => a.ClockinTime?.Date ?? DateTime.MinValue, new DateTimeComparer())
      .Select(group =>
      {
          var date = TimeZoneInfo.ConvertTimeFromUtc(group.Key, timeZoneInfo);
          // Get total logged time for the date
          var totalLoggedTime = group.Sum(a => a.Type == false ? (a.ClockoutTime - a.ClockinTime)?.TotalHours ?? 0 : 0);
          // Calculate metrics based on the sessions and daily working hours
          var metrics = CalculateTimeMetrics(employeeId, group.Key);
          // Get HoursPerDay from workschedule
          var hoursPerDay = GetHoursPerDayFromWorkSchedule(employee.FkScheduleId);
          return new
          {
              Date = date,
              TotalLoggedTime = totalLoggedTime,
              PaidTime = metrics.PaidTime,
              Overtime = metrics.Overtime,
              DeficitTime = metrics.DeficitTime,
              HoursPerDay = hoursPerDay,
              Sessions = GetLoggedSessionDetails(group.ToList(), timeZoneInfo)
          };
      })
      .ToList();

                return Ok(loggedSessions);
            }
            catch (Exception ex)
            {
                // Log the exception details
                // Log.Error(ex, "An error occurred while processing the request.");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
        private double GetHoursPerDayFromWorkSchedule(int? scheduleId)
        {
            if (scheduleId.HasValue)
            {
                var workSchedule = _dbContext.Workschedules.Find(scheduleId.Value);
                if (workSchedule != null && workSchedule.HoursPerDay.HasValue)
                {
                    // Use TotalHours property of TimeSpan to convert it to double
                    return workSchedule.HoursPerDay.Value.TotalHours;
                }
            }
            return 0;
        }
        private (double PaidTime, double Overtime, double DeficitTime) CalculateTimeMetrics(int employeeId, DateTime date)
        {
            // Get the employee's work schedule ID
            int? scheduleId = _dbContext.Employees
                .Where(e => e.EmployeeId == employeeId)
                .Select(e => e.FkScheduleId)
                .FirstOrDefault();

            if (!scheduleId.HasValue)
            {
                // Handle the case where the employee has no assigned work schedule
                return (0, 0, 0);
            }

            // Get the daily working hours from the work schedule
            double dailyWorkingHours = GetHoursPerDayFromWorkSchedule(scheduleId);

            if (dailyWorkingHours <= 0)
            {
                // Handle the case where the daily working hours are not valid
                return (0, 0, 0);
            }

            // Retrieve logged sessions for the specified date, excluding sessions where Type is null
            var sessions = _dbContext.AttendanceSessions
                .Where(a => a.FkEmployeeId == employeeId &&
                            a.ClockinTime.HasValue &&
                            a.ClockinTime.Value.Date == date.Date &&
                            a.Type.HasValue)  // Exclude sessions where Type is null
                .OrderBy(a => a.ClockinTime)
                .ToList();

            // Calculate metrics based on the sessions and daily working hours
            double totalLoggedTime = 0;
            double paidTime = 0;
            double overtime = 0;
            double deficitTime = 0;

            foreach (var session in sessions)
            {
                var loggedTime = (session.ClockoutTime - session.ClockinTime)?.TotalHours ?? 0;

                if (session.Type == false)
                {
                    totalLoggedTime += loggedTime;

                    // Determine if the logged time is within the paid time or overtime
                    if (loggedTime <= dailyWorkingHours)
                    {
                        paidTime += loggedTime;
                    }
                    else
                    {
                        overtime += loggedTime - dailyWorkingHours;
                    }
                }
                else
                {
                    // Handle clock-out sessions if needed
                }
            }

            // Calculate deficit time if applicable
            deficitTime = dailyWorkingHours - totalLoggedTime;

            return (paidTime, overtime, deficitTime);
        }


        // Define the DateTimeComparer class
        public class DateTimeComparer : IEqualityComparer<DateTime> 
        {
            public bool Equals(DateTime x, DateTime y)
            {
                return x.Date == y.Date;
            }

            public int GetHashCode(DateTime obj)
            {
                return obj.Date.GetHashCode();
            }
        }
        [HttpGet("MonthlySummary/{employeeId}")]
        public IActionResult GetMonthlySummary(int employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var employee = _dbContext.Employees.Find(employeeId);
                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                var timeZoneInfo = GetTimeZoneInfoFromRequestHeaders(HttpContext.Request.Headers);

                // Get the work schedule for the employee
                var workSchedule = _dbContext.Workschedules.Find(employee.FkScheduleId);
                if (workSchedule == null)
                {
                    return NotFound("Work schedule not found for the employee.");
                }

                // Extract working hours from the workschedule
                double dailyWorkingHours = workSchedule.HoursPerDay?.TotalHours ?? 0;

                // Calculate total working hours for the month
                int totalWorkingDaysInMonth = CalculateTotalWorkingDays(startDate, endDate, workSchedule.WorkingDays);
                double totalWorkingHoursInMonth = totalWorkingDaysInMonth * dailyWorkingHours;

                // Initialize variables to store aggregated data
                double totalLoggedTime = 0;
                double totalPaidTime = 0;
                double totalOvertime = 0;

                // Loop through each day in the specified range
                for (DateTime currentDate = startDate.Date; currentDate <= endDate.Date; currentDate = currentDate.AddDays(1))
                {
                    // Calculate metrics for each day
                    var metrics = CalculateTimeMetrics(employeeId, currentDate, timeZoneInfo, dailyWorkingHours);

                    // Aggregate the metrics
                    totalLoggedTime += metrics.totalLoggedTime;
                    totalPaidTime += metrics.paidTime;
                    totalOvertime += metrics.overtime;
                }

                // Calculate deficit time
                double totalDeficitTime = Math.Max(0, totalWorkingHoursInMonth - totalLoggedTime);

                // Convert the total times to hours and minutes
                var monthlySummary = new
                {
                    TotalLoggedTime = ConvertToHoursAndMinutes(totalLoggedTime),
                    TotalPaidTime = ConvertToHoursAndMinutes(totalPaidTime),
                    TotalOvertime = ConvertToHoursAndMinutes(totalOvertime),
                    TotalDeficitTime = ConvertToHoursAndMinutes(totalDeficitTime),
                    TotalWorkingHours = ConvertToHoursAndMinutes(totalWorkingHoursInMonth)
                };

                return Ok(monthlySummary);
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        // Helper method to convert total time to hours and minutes
        private string ConvertToHoursAndMinutes(double totalHours)
        {
            TimeSpan timeSpan = TimeSpan.FromHours(totalHours);
            return $"{(int)timeSpan.TotalHours} hr {timeSpan.Minutes} min";
        }

        // Rest of the code remains unchanged


        private int CalculateTotalWorkingDays(DateTime startDate, DateTime endDate, string workingDaysJson)
{
    var workingDays = ParseWorkingDays(workingDaysJson);
    int totalWorkingDays = 0;
    
    for (DateTime currentDate = startDate.Date; currentDate <= endDate.Date; currentDate = currentDate.AddDays(1))
    {
        int dayOfWeek = (int)currentDate.DayOfWeek;

        // Adjust dayOfWeek to match the given convention (1 for Monday, 2 for Tuesday, ..., 7 for Sunday)
        if (dayOfWeek == 0)
        {
            dayOfWeek = 7; // Sunday is represented as 7 in your convention
        }

        if (workingDays.ContainsKey(dayOfWeek) && workingDays[dayOfWeek])
        {
            totalWorkingDays++;
        }
    }

    return totalWorkingDays;
}

        private Dictionary<int, bool> ParseWorkingDays(string workingDaysJson)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, bool>>(workingDaysJson);
            }
            catch (Exception)
            {
                // Log or handle the exception appropriately
                return new Dictionary<int, bool>();
            }
        }
        private (double totalLoggedTime, double paidTime, double overtime, double deficitTime) CalculateTimeMetrics(int employeeId, DateTime date, TimeZoneInfo timeZoneInfo, double dailyWorkingHours)
        {
            // Retrieve logged sessions for the specified date
            var sessions = _dbContext.AttendanceSessions
                .Where(a => a.FkEmployeeId == employeeId &&
                            a.ClockinTime.HasValue &&
                            a.ClockinTime.Value.Date == date.Date)
                .OrderBy(a => a.ClockinTime)
                .ToList();
            // Initialize variables
            double totalLoggedTime = 0;
            double paidTime = 0;
            double overtime = 0;
            double deficitTime = 0;
            foreach (var session in sessions)
            {
                var loggedTime = (session.ClockoutTime - session.ClockinTime)?.TotalHours ?? 0;
                if (session.Type == false)
                {
                    totalLoggedTime += loggedTime;
                    // Determine if the logged time is within the paid time or overtime
                    if (loggedTime <= dailyWorkingHours)
                    {
                        paidTime += loggedTime;
                    }
                    else
                    {
                        overtime += loggedTime - dailyWorkingHours;
                    }
                }
                else
                {
                    // Handle clock-out sessions if needed
                }
            }
            // Calculate deficit time if applicable
            deficitTime = dailyWorkingHours - totalLoggedTime;
            return (totalLoggedTime, paidTime, overtime, deficitTime);
        }
        [HttpGet("AllEmployeeMonthlySummary")]
        public IActionResult GetAllEmployeesMonthlySummary(DateTime startDate, DateTime endDate)
        {
            try
            {
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
                var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType);
                var lineManagerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                IQueryable<Employee> employeesQuery;

                if (roleClaim != null && roleClaim.Value == "Superadmin")
                {
                    // SuperAdmin role, retrieve all employees for the company
                    employeesQuery = _dbContext.Employees.Where(e => e.FkCompanyId == companyId);
                }
                else if (lineManagerIdClaim != null && int.TryParse(lineManagerIdClaim.Value, out int EmployeeId))
                {
                    // LineManager role, retrieve employees managed by the LineManager
                    employeesQuery = _dbContext.Employees
                        .Where(e => e.FkCompanyId == companyId && e.LineManagerId == EmployeeId);
                }
                else
                {
                    // Other roles, retrieve data for the user's own employee ID
                    var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");
                    if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int employeeId))
                    {
                        return BadRequest("Invalid data. Employee information is missing.");
                    }

                    employeesQuery = _dbContext.Employees.Where(e => e.EmployeeId == employeeId);
                }

                var employees = employeesQuery.ToList();

                var monthlySummaries = employees.Select(employee =>
                {
                    var timeZoneInfo = GetTimeZoneInfoFromRequestHeaders(HttpContext.Request.Headers);
                    var monthlySummary = CalculateMonthlySummary(employee.EmployeeId, startDate, endDate, timeZoneInfo);
                    return new
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = employee.FullName,
                        MonthlySummary = monthlySummary
                    };
                }).ToList();

                return Ok(monthlySummaries);
            }
            catch (Exception ex)
            {
                // Log the exception details
                // Log.Error(ex, "An error occurred while processing the request.");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        private object CalculateMonthlySummary(int employeeId, DateTime startDate, DateTime endDate, TimeZoneInfo timeZoneInfo)
        {
            var employee = _dbContext.Employees.Find(employeeId);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var workSchedule = _dbContext.Workschedules.Find(employee.FkScheduleId);
            if (workSchedule == null)
            {
                return NotFound("Work schedule not found for the employee.");
            }

            double dailyWorkingHours = workSchedule.HoursPerDay?.TotalHours ?? 0;
            int totalWorkingDaysInMonth = CalculateTotalWorkingDays(startDate, endDate, workSchedule.WorkingDays);
            double totalWorkingHoursInMonth = totalWorkingDaysInMonth * dailyWorkingHours;

            double totalLoggedTime = 0;
            double totalPaidTime = 0;
            double totalOvertime = 0;

            for (DateTime currentDate = startDate.Date; currentDate <= endDate.Date; currentDate = currentDate.AddDays(1))
            {
                var metrics = CalculateTimeMetrics(employeeId, currentDate, timeZoneInfo, dailyWorkingHours);

                totalLoggedTime += metrics.totalLoggedTime;
                totalPaidTime += metrics.paidTime;
                totalOvertime += metrics.overtime;
            }

            double totalDeficitTime = Math.Max(0, totalWorkingHoursInMonth - totalLoggedTime);

            var monthlySummary = new
            {
                TotalLoggedTime = ConvertToHoursAndMinutes(totalLoggedTime),
                TotalPaidTime = ConvertToHoursAndMinutes(totalPaidTime),
                TotalOvertime = ConvertToHoursAndMinutes(totalOvertime),
                TotalDeficitTime = ConvertToHoursAndMinutes(totalDeficitTime),
                TotalWorkingHours = ConvertToHoursAndMinutes(totalWorkingHoursInMonth)
            };
            return monthlySummary;
        }

        public class AttendanceClockInRequest
        {
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public int FkOfficeId { get; set; }
        }
        public class AttendanceClockOutRequest
        {
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public int FkOfficeId { get; set; }
        }
     
    }
}
