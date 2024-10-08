using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public string? Description { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public string? Domain { get; set; }

    public string? Size { get; set; }

    public byte[]? Logo { get; set; }

    public int? FkIndustryId { get; set; }

    public string? Language { get; set; }

    public long? ContactNumber { get; set; }

    public string? ContactEmail { get; set; }

    public string? CompanyWebsite { get; set; }

    public int? FkSubscriptionId { get; set; }

    public int? FkPermissionId { get; set; }

    public int? FkRemainderId { get; set; }

    public int? FkContractTypeId { get; set; }

    public int? FkCompanyId { get; set; }

    public Guid? CompanyGuid { get; set; }

    public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ContractType? FkContractType { get; set; }

    public virtual Industry? FkIndustry { get; set; }

    public virtual Reminder? FkRemainder { get; set; }

    public virtual Subscription? FkSubscription { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<LeaveType> LeaveTypes { get; set; } = new List<LeaveType>();

    public virtual ICollection<News> News { get; set; } = new List<News>();

    public virtual ICollection<OffLocation> OffLocations { get; set; } = new List<OffLocation>();

    public virtual ICollection<SectionPermission> SectionPermissions { get; set; } = new List<SectionPermission>();

    public virtual ICollection<Stage> Stages { get; set; } = new List<Stage>();

    public virtual ICollection<Workschedule> Workschedules { get; set; } = new List<Workschedule>();
}
