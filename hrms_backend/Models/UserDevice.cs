using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class UserDevice
{
    public int DeviceId { get; set; }

    public int? FkUserId { get; set; }

    public string? DeviceName { get; set; }

    public string? DeviceType { get; set; }

    public string? DeviceToken { get; set; }
}
