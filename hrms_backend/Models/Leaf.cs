using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Leaf
{
    public int LeaveId { get; set; }

    public int? FkEmployeeId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public TimeSpan? ActualTotalLeaves { get; set; }

    public string? Attachment { get; set; }

    public int? Status { get; set; }
    public int? total_days { get; set; }
    public bool? IsActive { get; set; }

    public int? FkLeaveTypeId { get; set; }

    public string? Note { get; set; }

    public int? FkScheduleId { get; set; }

    public virtual Employee? FkEmployee { get; set; }

    public virtual LeaveType? FkLeaveType { get; set; }
}
