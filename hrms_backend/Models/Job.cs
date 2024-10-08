using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Job
{
    public int JobId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public int? FkEmployeeId { get; set; }

    public int? FkCompanyId { get; set; }

    public DateTime? ProbationStartDate { get; set; }

    public DateTime? ProbationEndDate { get; set; }

    public string? JobTittle { get; set; }

    public string? EmployeementType { get; set; }

    public string? DepartmentName { get; set; }

    public string? OfficeName { get; set; }

    public int? Quantity { get; set; }

    public DateTime? ClosingDate { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();

    public virtual Company? FkCompany { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
