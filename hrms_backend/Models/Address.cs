using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Address
{
    public int AddressId { get; set; }

    public int? FkEmployeeId { get; set; }

    public string? PrimaryAddress { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? StateProvince { get; set; }

    public string? PostalCode { get; set; }

    public string? PermanentAddress { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
