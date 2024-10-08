using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class BankInformation
{
    public int BankInfoId { get; set; }

    public int? FkEmployeeId { get; set; }

    public string? BankName { get; set; }

    public string? AccountName { get; set; }

    public string? Branch { get; set; }

    public string? AccountNumber { get; set; }

    public string? SwiftBic { get; set; }

    public string? Iban { get; set; }

    public string? IfscCode { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
