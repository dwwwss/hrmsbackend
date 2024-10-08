using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class SectionPermission
{
    public int SectionPermissionId { get; set; }

    public int RoleId { get; set; }

    public int SectionId { get; set; }

    public int PermissionFlag { get; set; }

    public int? CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Section Section { get; set; } = null!;
}
