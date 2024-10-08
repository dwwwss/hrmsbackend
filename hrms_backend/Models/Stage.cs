using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Stage
{
    public int StageId { get; set; }

    public string StageName { get; set; } = null!;

    public int? FkCompanyId { get; set; }

    public virtual Company? FkCompany { get; set; }
}
