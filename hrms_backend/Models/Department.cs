using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public int? FkDesignationId { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
