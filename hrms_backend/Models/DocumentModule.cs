using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class DocumentModule
{
    public int DocumentModuleId { get; set; }

    public string? Name { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Description { get; set; }

    public bool? Share { get; set; }

    public string? SharedWith { get; set; }

    public int? NumberOfFiles { get; set; }

    public decimal? Size { get; set; }

    public int? FkCompanyId { get; set; }

    public virtual ICollection<DocumentFile> DocumentFiles { get; set; } = new List<DocumentFile>();

    public virtual Company? FkCompany { get; set; }
}
