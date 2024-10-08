using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class EmployeeDocument
{
    public int? EmployeeDocumentId { get; set; }

    public int? FkEmployeeId { get; set; }

    public int? FkDocumentId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Document? FkDocument { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
