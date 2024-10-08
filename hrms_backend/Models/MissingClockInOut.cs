using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class MissingClockInOut
{
    public int MissingclockinoutId { get; set; }

    public DateTime? Date { get; set; }

    public TimeSpan? MissingClockInTime { get; set; }

    public TimeSpan? MissingClockOutTime { get; set; }
}
