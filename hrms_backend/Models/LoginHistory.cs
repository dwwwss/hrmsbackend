using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class LoginHistory
{
    public int LoginId { get; set; }

    public int? FkEmployeeId { get; set; }

    public DateTime? LoginDate { get; set; }

    public DateTime? LogoutDate { get; set; }

    public string? LoginStatus { get; set; }
}
