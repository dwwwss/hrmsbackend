using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class EmployeeStatus
{
    public int EmpStatusId { get; set; }

    public string? StatusName { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
