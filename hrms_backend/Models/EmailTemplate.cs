using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class EmailTemplate
{
    public int TemplateId { get; set; }

    public string? Subject { get; set; }

    public string? Stage { get; set; }

    public DateTime? LastModified { get; set; }

    public string? Body { get; set; }

    public int? FkCompanyId { get; set; }

    public string? EmailTemplate1 { get; set; }
}
