using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class ActivityLog
{
    public int LogId { get; set; }

    public int? FkUserId { get; set; }

    public DateTime? ActivityDate { get; set; }

    public string? ActivityDescription { get; set; }
}
