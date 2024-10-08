using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Office
{
    public string? OfficeName { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? PostalCode { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public int? FkCompanyId { get; set; }

    public int? FkScheduleId { get; set; }

    public string? FkOfficelocationId { get; set; }

    public long? ContactNo { get; set; }

    public string? Email { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public double? Radius { get; set; }

    public string? Qrcode { get; set; }

    public int OfficeId { get; set; }

    public int? FkCountryId { get; set; }

    public int? FkStateId { get; set; }

    public int? FkCityId { get; set; }

    public virtual ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
