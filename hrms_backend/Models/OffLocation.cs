using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class OffLocation
{
    public int OfficeLocationId { get; set; }

    public string? OfficeName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Geofencing { get; set; }

    public double? Latitude1 { get; set; }

    public double? Longitude1 { get; set; }

    public byte[]? Qrcode { get; set; }

    public int? Fkcompanyid { get; set; }

    public string? Qrcodeimage { get; set; }

    public virtual Company? Fkcompany { get; set; }
}
