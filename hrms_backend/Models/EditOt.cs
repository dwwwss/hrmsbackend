using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class EditOt
{
    public int EditotId { get; set; }

    public DateTime? Date { get; set; }

    public TimeSpan? OriginalOtHours { get; set; }

    public TimeSpan? EditedOtHours { get; set; }

    public string? Reason { get; set; }
}
