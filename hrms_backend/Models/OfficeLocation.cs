using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class OfficeLocation
{
    public int OfficelocationId { get; set; }

    public string? OfficeName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public decimal? Geofencing { get; set; }

    public decimal? Latitude1 { get; set; }

    public decimal? Longitude1 { get; set; }

    public byte[]? Qrcode { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}
