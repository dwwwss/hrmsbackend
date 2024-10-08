using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class EmergencyContact
{
    public int EmergencyContactId { get; set; }

    public int? FkEmployeeId { get; set; }

    public string? FullName { get; set; }

    public string? Relationship { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
