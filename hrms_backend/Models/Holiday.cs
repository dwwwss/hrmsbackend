using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Holiday
{
    public int HolidayId { get; set; }

    public string? HolidayName { get; set; }

    public int? FkCompanyId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
