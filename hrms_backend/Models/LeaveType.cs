using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class LeaveType
{
    public int LeaveTypeId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsPaid { get; set; }

    public int? Count { get; set; }

    public string? Duration { get; set; }

    public int? Limit { get; set; }

    public bool? IsCarryForward { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? EligibleEmployeeType { get; set; }

    public string? SpecificEmployees { get; set; }

    public int? FkCompanyId { get; set; }

    public virtual Company? FkCompany { get; set; }

    public virtual ICollection<Leaf> Leaves { get; set; } = new List<Leaf>();
}
