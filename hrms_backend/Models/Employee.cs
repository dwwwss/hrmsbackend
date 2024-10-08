using System;
using System.Collections.Generic;

namespace hrms_backend.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public int? FkDepartmentId { get; set; }

    public int? LineManagerId { get; set; }

    public int? FkCompanyId { get; set; }

    public int? FkUserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? FullName { get; set; }

    public string? Gender { get; set; }

    public string? DateOfBirth { get; set; }

    public string? MaritalStatus { get; set; }

    public string? Nationality { get; set; }

    public int? PersonalTaxId { get; set; }

    public string? SocialInsurance { get; set; }

    public string? HealthInsurance { get; set; }

    public string? PhoneNumber { get; set; }

    public string? MarriageAnniversary { get; set; }

    public string? AlternateMobileNo { get; set; }

    public bool? IsActive { get; set; }

    public int? FkEmpstatusId { get; set; }

    public string? FkLoginHistoryId { get; set; }

    public int? FkOfficeId { get; set; }

    public int? FkEmployeeGroupId { get; set; }

    public byte[]? FeaturedImage { get; set; }

    public string? CurrentState { get; set; }

    public int? CurrentSessionId { get; set; }

    public string? ServiceYear { get; set; }

    public string? Image { get; set; }

    public int? Ids { get; set; }

    public DateTime? JoinDate { get; set; }

    public int? fk_role_id { get; set; }

    public int? TenantId { get; set; }

    public byte[]? Fimage { get; set; }

    public int? FkDesignationId { get; set; }

    public bool? IsVerified { get; set; }

    public string? ResetToken { get; set; }

    public string? ActivationToken { get; set; }

    public int? IsPasswordGenerated { get; set; }

    public int? FkScheduleId { get; set; }

    public int? FkEmployementTypeId { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<BankInformation> BankInformations { get; set; } = new List<BankInformation>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();

    public virtual Company? FkCompany { get; set; }

    public virtual Department? FkDepartment { get; set; }

    public virtual Designation? FkDesignation { get; set; }

    public virtual EmployeeGroup? FkEmployeeGroup { get; set; }

    public virtual EmployementType? FkEmployementType { get; set; }

    public virtual EmployeeStatus? FkEmpstatus { get; set; }

    public virtual Office? FkOffice { get; set; }

    public virtual Role? FkRole { get; set; }

    public virtual Workschedule? FkSchedule { get; set; }

    public virtual ICollection<JobHistory> JobHistories { get; set; } = new List<JobHistory>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<Leaf> Leaves { get; set; } = new List<Leaf>();

    public virtual ICollection<News> News { get; set; } = new List<News>();
}
