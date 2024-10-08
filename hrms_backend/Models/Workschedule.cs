using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Workschedule
{
    public string? ScheduleName { get; set; }

    public string? Description { get; set; }

    public TimeSpan? HoursPerDay { get; set; }

    public string? ScheduleType { get; set; }

    public string? HoursPerWeek { get; set; }

    public int? FkCompanyId { get; set; }

    public string? DailyWorkingHours { get; set; }

    public string? WorkingDays { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public TimeSpan? LateTime { get; set; }

    public TimeSpan? HalfDayTime { get; set; }

 /*   public bool? IsDefault { get; set; }*/

    public int ScheduleId { get; set; }

  /*  public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();*/

    public virtual Company? FkCompany { get; set; }
}
