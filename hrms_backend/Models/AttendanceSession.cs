using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class AttendanceSession
{
    public int AtttendnaceSessionId { get; set; }

    public DateTime? ClockinTime { get; set; }

    public DateTime? ClockoutTime { get; set; }

    public bool? Type { get; set; }

    public decimal? ClockInLatitude { get; set; }

    public decimal? ClockInLongitude { get; set; }

    public int? FkOfficeId { get; set; }

    public int? FkEmployeeId { get; set; }

    public bool? Clockingeofence { get; set; }

    public decimal? ClockOutLatitude { get; set; }

    public decimal? ClockOutLongitude { get; set; }

    public bool? ClockOutGeofence { get; set; }

    public virtual Employee? FkEmployee { get; set; }

    public virtual Office? FkOffice { get; set; }
}
