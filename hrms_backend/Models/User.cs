using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? FkRoleId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public long? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public int? FkCompanyId { get; set; }

    public int? FkLoginHistoryId { get; set; }

    public int? CurrentSessionId { get; set; }

    public byte[]? FeaturedImage { get; set; }

    public int? TenantId { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

  

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Company? FkCompany { get; set; }

    public virtual LoginHistory? FkLoginHistory { get; set; }

    public virtual Role? FkRole { get; set; }

    public virtual ICollection<Leaf> Leaves { get; set; } = new List<Leaf>();

    public virtual ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();
}
