using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class EditPaidTime
{
    public int EditId { get; set; }

    public DateTime? Date { get; set; }

    public TimeSpan? OriginalPaidTime { get; set; }

    public TimeSpan? EditedPaidTime { get; set; }

    public string? Reason { get; set; }
}
