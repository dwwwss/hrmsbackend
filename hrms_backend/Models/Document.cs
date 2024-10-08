using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Document
{
    public int DocumentId { get; set; }

    public int? FkEmployeeId { get; set; }

    public string? DocumentType { get; set; }

    public string? DocumentName { get; set; }

    public string? FilePath { get; set; }

    public DateTime? UploadedDate { get; set; }

    public string? UploadedBy { get; set; }

    public bool? IsActive { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
