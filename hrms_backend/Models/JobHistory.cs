using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class JobHistory
{
    public int JobhistoryId { get; set; }

    public int? FkJobId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? JobTitle { get; set; }

    public string? PositionType { get; set; }

    public string? EmploymentType { get; set; }

    public string? Office { get; set; }

    public string? Note { get; set; }

    public int? FkEmployeeId { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
