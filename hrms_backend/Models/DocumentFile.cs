using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class DocumentFile
{
    public int DocumentFileId { get; set; }

    public string? FileName { get; set; }

    public long? Size { get; set; }

    public string? FilePath { get; set; }

    public int? DocumentModuleId { get; set; }

    public virtual DocumentModule? DocumentModule { get; set; }
}
