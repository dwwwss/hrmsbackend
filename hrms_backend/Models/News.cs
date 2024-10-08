using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class News
{
    public int NewsId { get; set; }

    public string Title { get; set; } = null!;

    public string? Imageurl { get; set; }

    public string? Content { get; set; }

    public DateTime? PublishedDate { get; set; }

    public int? FkEmployeeId { get; set; }

    public int? FkCompanyId { get; set; }

    public virtual Company? FkCompany { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
