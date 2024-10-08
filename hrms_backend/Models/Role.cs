using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public int? CompanyId { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<RoleModulePermissionMapping> RoleModulePermissionMappings { get; set; } = new List<RoleModulePermissionMapping>();

    public virtual ICollection<SectionPermission> SectionPermissions { get; set; } = new List<SectionPermission>();
}
