using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Section
{
    public int SectionId { get; set; }

    public string SectionName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<SectionPermission> SectionPermissions { get; set; } = new List<SectionPermission>();
}
