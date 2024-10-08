using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class RoleModulePermissionMapping
{
    public int MappingId { get; set; }

    public int? RoleId { get; set; }

    public int? ModuleId { get; set; }

    public int? PermissionFlag { get; set; }

    public virtual Module? Module { get; set; }

    public virtual Role? Role { get; set; }
}
