using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Designation
{
    public int DesignationId { get; set; }

    public string? DesignationName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
