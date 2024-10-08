using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Module
{
    public int ModuleId { get; set; }

    public string ModuleName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<RoleModulePermissionMapping> RoleModulePermissionMappings { get; set; } = new List<RoleModulePermissionMapping>();
}
