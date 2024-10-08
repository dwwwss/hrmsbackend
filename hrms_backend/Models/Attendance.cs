using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int FkEmployeeId { get; set; }

    public bool? Type { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int? FkOfficeId { get; set; }

    public DateTime? Datetime { get; set; }

    public bool? IsGeofence { get; set; }

    public int? ParentAttendanceId { get; set; }

    public virtual Employee FkEmployee { get; set; } = null!;

    public virtual Office? FkOffice { get; set; }
}
