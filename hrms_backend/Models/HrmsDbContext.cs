using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace hrms_backend.Models;

public partial class HrmsDbContext : DbContext
{
    public HrmsDbContext()
    {
    }

    public HrmsDbContext(DbContextOptions<HrmsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AttendanceSession> AttendanceSessions { get; set; }

    public virtual DbSet<BankInformation> BankInformations { get; set; }

    public virtual DbSet<Candidate> Candidates { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<ContractType> ContractTypes { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Designation> Designations { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<EditOt> EditOts { get; set; }

    public virtual DbSet<EditPaidTime> EditPaidTimes { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<EmergencyContact> EmergencyContacts { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

    public virtual DbSet<EmployeeGroup> EmployeeGroups { get; set; }

    public virtual DbSet<EmployeeStatus> EmployeeStatuses { get; set; }

    public virtual DbSet<EmployementType> EmployementTypes { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<Industry> Industries { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobHistory> JobHistories { get; set; }

    public virtual DbSet<Leaf> Leaves { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<MissingClockInOut> MissingClockInOuts { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Nationality> Nationalities { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<OffLocation> OffLocations { get; set; }

    public virtual DbSet<Office> Offices { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleModulePermissionMapping> RoleModulePermissionMappings { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<SectionPermission> SectionPermissions { get; set; }

    public virtual DbSet<Stage> Stages { get; set; }

    public virtual DbSet<Statess> Statesses { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<UserDevice> UserDevices { get; set; }

    public virtual DbSet<Workschedule> Workschedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=10.0.0.168,1433;Database=HRMS_DB;User Id=rishi;Password=123456;Encrypt=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Activity__9E2397E0FBE43A42");

            entity.Property(e => e.LogId)
                .ValueGeneratedNever()
                .HasColumnName("log_id");
            entity.Property(e => e.ActivityDate)
                .HasColumnType("datetime")
                .HasColumnName("activity_date");
            entity.Property(e => e.ActivityDescription)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("activity_description");
            entity.Property(e => e.FkUserId).HasColumnName("fk_user_id");
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Address__CAA247C86F05D4A0");

            entity.ToTable("Address");

            entity.Property(e => e.AddressId)
                .ValueGeneratedNever()
                .HasColumnName("address_id");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("country");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.PermanentAddress)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("permanent_address");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("postal_code");
            entity.Property(e => e.PrimaryAddress)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("primary_address");
            entity.Property(e => e.StateProvince)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("state_province");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.FkEmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Address_Employee");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__20D6A9683231AAE7");

            entity.ToTable("Attendance");

            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.Datetime)
                .HasColumnType("datetime")
                .HasColumnName("datetime");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.FkOfficeId).HasColumnName("fk_office_id");
            entity.Property(e => e.IsGeofence).HasColumnName("is_geofence");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("longitude");
            entity.Property(e => e.ParentAttendanceId).HasColumnName("parent_attendance_id");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.FkEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Employee");

            entity.HasOne(d => d.FkOffice).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.FkOfficeId)
                .HasConstraintName("FK_Attendance_Offices");
        });

        modelBuilder.Entity<AttendanceSession>(entity =>
        {
            entity.HasKey(e => e.AtttendnaceSessionId).HasName("PK__Attendan__1DCC4B888F5F2789");

            entity.ToTable("AttendanceSession");

            entity.Property(e => e.AtttendnaceSessionId).HasColumnName("atttendnace_session_id");
            entity.Property(e => e.ClockInLatitude)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("clock_in_latitude");
            entity.Property(e => e.ClockInLongitude)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("clock_in_longitude");
            entity.Property(e => e.ClockOutLatitude)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("clock_out_latitude");
            entity.Property(e => e.ClockOutLongitude)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("clock_out_longitude");
            entity.Property(e => e.ClockinTime)
                .HasColumnType("datetime")
                .HasColumnName("clockin_time");
            entity.Property(e => e.Clockingeofence).HasColumnName("clockingeofence");
            entity.Property(e => e.ClockoutTime)
                .HasColumnType("datetime")
                .HasColumnName("clockout_time");
            entity.Property(e => e.ClockOutGeofence).HasColumnName("clockoutgeofence");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.FkOfficeId).HasColumnName("fk_office_id");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.AttendanceSessions)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("fk_attendance_session_employee");

            entity.HasOne(d => d.FkOffice).WithMany(p => p.AttendanceSessions)
                .HasForeignKey(d => d.FkOfficeId)
                .HasConstraintName("FK_AttendanceSession_Offices");
        });

        modelBuilder.Entity<BankInformation>(entity =>
        {
            entity.HasKey(e => e.BankInfoId).HasName("PK__BankInfo__DABDF864033B341F");

            entity.ToTable("BankInformation");

            entity.Property(e => e.BankInfoId)
                .ValueGeneratedNever()
                .HasColumnName("bank_info_id");
            entity.Property(e => e.AccountName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("account_name");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("account_number");
            entity.Property(e => e.BankName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("bank_name");
            entity.Property(e => e.Branch)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("branch");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.Iban)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("iban");
            entity.Property(e => e.IfscCode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ifsc_code");
            entity.Property(e => e.SwiftBic)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("swift_bic");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.BankInformations)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK_BankInformation_Employee");
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.ToTable("Candidate");

            entity.Property(e => e.CandidateId).HasColumnName("candidate_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Cv).HasColumnName("cv");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.FkJobId).HasColumnName("fk_job_id");
            entity.Property(e => e.Fullname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("fullname");
            entity.Property(e => e.JoinDate)
                .HasColumnType("date")
                .HasColumnName("join_date");
            entity.Property(e => e.MobileNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("mobile_no");
            entity.Property(e => e.ProfilePhoto)
                .IsUnicode(false)
                .HasColumnName("profile_photo");
            entity.Property(e => e.Resume).IsUnicode(false);
            entity.Property(e => e.Skills)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Source)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("source");
            entity.Property(e => e.StagesName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("stages_name");

            entity.HasOne(d => d.FkCompany).WithMany(p => p.Candidates)
                .HasForeignKey(d => d.FkCompanyId)
                .HasConstraintName("FK_Candidate_Company");

            entity.HasOne(d => d.FkJob).WithMany(p => p.Candidates)
                .HasForeignKey(d => d.FkJobId)
                .HasConstraintName("FK_Candidate_Candidate");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cities__3213E83F98E1419B");

            entity.ToTable("cities");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.State).WithMany(p => p.Cities)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cities_statess");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCAF71E5AC2");

            entity.Property(e => e.FkCandidateId).HasColumnName("fk_candidate_id");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");

            entity.HasOne(d => d.FkCandidate).WithMany(p => p.Comments)
                .HasForeignKey(d => d.FkCandidateId)
                .HasConstraintName("FK_Comments_Candidate");

            entity.HasOne(d => d.FkCompany).WithMany(p => p.Comments)
                .HasForeignKey(d => d.FkCompanyId)
                .HasConstraintName("FK_Comments_Company");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Comments)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK_Comments_Employee");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Company__3E2672351D3755F2");

            entity.ToTable("Company");

            entity.Property(e => e.CompanyId)
                .ValueGeneratedNever()
                .HasColumnName("company_id");
            entity.Property(e => e.CompanyGuid).HasColumnName("company_guid");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("company_name");
            entity.Property(e => e.CompanyWebsite)
                .IsUnicode(false)
                .HasColumnName("company_website");
            entity.Property(e => e.ContactEmail)
                .IsUnicode(false)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactNumber).HasColumnName("contact_number");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Domain)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("domain");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.FkContractTypeId).HasColumnName("fk_contract_type_id");
            entity.Property(e => e.FkIndustryId).HasColumnName("fk_industry_id");
            entity.Property(e => e.FkPermissionId).HasColumnName("fk_permission_id");
            entity.Property(e => e.FkRemainderId).HasColumnName("fk_remainder_id");
            entity.Property(e => e.FkSubscriptionId).HasColumnName("fk_subscription_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Language)
                .IsUnicode(false)
                .HasColumnName("language");
            entity.Property(e => e.Logo)
                .HasColumnType("image")
                .HasColumnName("logo");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.Size)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("size");

            entity.HasOne(d => d.FkContractType).WithMany(p => p.Companies)
                .HasForeignKey(d => d.FkContractTypeId)
                .HasConstraintName("FK_Company_ContractTypes");

            entity.HasOne(d => d.FkIndustry).WithMany(p => p.Companies)
                .HasForeignKey(d => d.FkIndustryId)
                .HasConstraintName("FK_Company_Industry");

            entity.HasOne(d => d.FkRemainder).WithMany(p => p.Companies)
                .HasForeignKey(d => d.FkRemainderId)
                .HasConstraintName("FK_Company_Reminders");

            entity.HasOne(d => d.FkSubscription).WithMany(p => p.Companies)
                .HasForeignKey(d => d.FkSubscriptionId)
                .HasConstraintName("FK_Company_Subscription");
        });

        modelBuilder.Entity<ContractType>(entity =>
        {
            entity.HasKey(e => e.ContractTypeId).HasName("PK__Contract__B66967DF66A70ACD");

            entity.Property(e => e.ContractTypeId)
                .ValueGeneratedNever()
                .HasColumnName("contract_type_id");
            entity.Property(e => e.ContractTypeName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contract_type_name");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__countrie__3213E83F4465402F");

            entity.ToTable("countries");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Phonecode).HasColumnName("phonecode");
            entity.Property(e => e.Shortname)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("shortname");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C22324221B0A5495");

            entity.ToTable("Department");

            entity.Property(e => e.DepartmentId)
                .ValueGeneratedNever()
                .HasColumnName("department_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("department_name");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.FkDesignationId).HasColumnName("fk_designation_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(e => e.DesignationId).HasName("PK__Designat__177649C1BE733CB2");

            entity.ToTable("Designation");

            entity.Property(e => e.DesignationId)
                .ValueGeneratedNever()
                .HasColumnName("designation_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.DesignationName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("designation_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__9666E8AC1802A28D");

            entity.ToTable("Document");

            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentName).HasColumnName("document_name");
            entity.Property(e => e.DocumentType).HasColumnName("document_type");
            entity.Property(e => e.FilePath).HasColumnName("file_path");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");
            entity.Property(e => e.UploadedDate)
                .HasColumnType("date")
                .HasColumnName("uploaded_date");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Documents)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK_Document_Employee");
        });

        modelBuilder.Entity<EditOt>(entity =>
        {
            entity.HasKey(e => e.EditotId).HasName("PK__EditOT__234852D538753D43");

            entity.ToTable("EditOT");

            entity.Property(e => e.EditotId)
                .ValueGeneratedNever()
                .HasColumnName("editot_id");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.EditedOtHours).HasColumnName("edited_ot_hours");
            entity.Property(e => e.OriginalOtHours).HasColumnName("original_ot_hours");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("reason");
        });

        modelBuilder.Entity<EditPaidTime>(entity =>
        {
            entity.HasKey(e => e.EditId).HasName("PK__EditPaid__A8C1B4CC3C18EEAA");

            entity.ToTable("EditPaidTime");

            entity.Property(e => e.EditId)
                .ValueGeneratedNever()
                .HasColumnName("edit_id");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.EditedPaidTime).HasColumnName("edited_paid_time");
            entity.Property(e => e.OriginalPaidTime).HasColumnName("original_paid_time");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("reason");
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__EmailTem__F87ADD27755A01C1");

            entity.ToTable("EmailTemplate");

            entity.Property(e => e.EmailTemplate1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EmailTemplate");
            entity.Property(e => e.LastModified).HasColumnType("datetime");
        });

        modelBuilder.Entity<EmergencyContact>(entity =>
        {
            entity.HasKey(e => e.EmergencyContactId).HasName("PK__Emergenc__3B08826D8C394B6E");

            entity.ToTable("EmergencyContact");

            entity.Property(e => e.EmergencyContactId)
                .ValueGeneratedNever()
                .HasColumnName("emergency_contact_id");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("full_name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.Relationship)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("relationship");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.EmergencyContacts)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK_EmergencyContact_Employee");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__C52E0BA8B53E3551");

            entity.ToTable("Employee");

            entity.Property(e => e.EmployeeId)
                .ValueGeneratedNever()
                .HasColumnName("employee_id");
            entity.Property(e => e.ActivationToken)
                .IsUnicode(false)
                .HasColumnName("activation_token");
            entity.Property(e => e.AlternateMobileNo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("alternate_mobile_no");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrentSessionId).HasColumnName("current_session_id");
            entity.Property(e => e.CurrentState)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("current_state");
            entity.Property(e => e.DateOfBirth)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FeaturedImage)
                .HasColumnType("image")
                .HasColumnName("featured_image");
            entity.Property(e => e.Fimage).IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.FkDepartmentId).HasColumnName("fk_department_id");
            entity.Property(e => e.FkDesignationId).HasColumnName("fk_designation_id");
            entity.Property(e => e.FkEmployeeGroupId).HasColumnName("fk_employee_group_id");
            entity.Property(e => e.FkEmployementTypeId).HasColumnName("fk_employement_type_id");
            entity.Property(e => e.FkEmpstatusId).HasColumnName("fk_empstatus_id");
            entity.Property(e => e.FkLoginHistoryId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("fk_login_history_id");
            entity.Property(e => e.FkOfficeId).HasColumnName("fk_office_id");
            entity.Property(e => e.fk_role_id).HasColumnName("fk_role_id");
            entity.Property(e => e.FkUserId).HasColumnName("fk_user_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("gender");
            entity.Property(e => e.HealthInsurance)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("health_insurance");
            entity.Property(e => e.Ids).HasColumnName("IDS");
            entity.Property(e => e.Image)
                .IsUnicode(false)
                .HasColumnName("image");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.JoinDate)
                .HasColumnType("date")
                .HasColumnName("join_date");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.LineManagerId).HasColumnName("line_manager_id");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("marital_status");
            entity.Property(e => e.MarriageAnniversary)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("marriage_anniversary");
            entity.Property(e => e.MobileNo).HasColumnName("mobile_no");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.Nationality)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nationality");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.PersonalTaxId).HasColumnName("personal_tax_id");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.ResetToken).IsUnicode(false);
            entity.Property(e => e.ServiceYear)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("service_year");
            entity.Property(e => e.SocialInsurance)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("social_insurance");
            entity.Property(e => e.TenantId).HasColumnName("tenantId");

            entity.HasOne(d => d.FkCompany).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FkCompanyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Employee__fk_com__787EE5A0");

            entity.HasOne(d => d.FkDepartment).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FkDepartmentId)
                .HasConstraintName("FK__Employee__fk_dep__75A278F5");

            entity.HasOne(d => d.FkDesignation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FkDesignationId)
                .HasConstraintName("FK_Employee_Employee");

            entity.HasOne(d => d.FkEmployeeGroup).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FkEmployeeGroupId)
                .HasConstraintName("FK_Employee_EmployeeGroups");

            entity.HasOne(d => d.FkEmployementType).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FkEmployementTypeId)
                .HasConstraintName("FK_Employee_EmployementType");

            entity.HasOne(d => d.FkEmpstatus).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FkEmpstatusId)
                .HasConstraintName("FK__Employee__fk_emp__7A672E12");

            entity.HasOne(d => d.FkOffice).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FkOfficeId)
                .HasConstraintName("FK_Employee_Offices");

            entity.HasOne(d => d.FkRole).WithMany(p => p.Employees)
                .HasForeignKey(d => d.fk_role_id)
                .HasConstraintName("FK_Employee_Role");


            entity.Property(e => e.FkScheduleId).HasColumnName("fk_schedule_id");
            entity.HasOne(d => d.FkSchedule)
                .WithMany()
                .HasForeignKey(d => d.FkScheduleId)
                .HasConstraintName("FK_Employee_Workschedule");
        });

        modelBuilder.Entity<EmployeeDocument>(entity =>
        {
            entity.HasKey(e => e.EmployeeDocumentId); // Set EmployeeDocumentId as the primary key

            entity.Property(e => e.EmployeeDocumentId)
                .HasColumnName("employee_document_id");
            entity.Property(e => e.FkDocumentId).HasColumnName("fk_document_id");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasOne(d => d.FkDocument).WithMany()
                .HasForeignKey(d => d.FkDocumentId)
                .HasConstraintName("FK_EmployeeDocuments_Document");

            entity.HasOne(d => d.FkEmployee).WithMany()
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK_EmployeeDocuments_Employee");
        });

        modelBuilder.Entity<EmployeeGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Employee__D57795A05F39F93C");

            entity.Property(e => e.GroupId)
                .ValueGeneratedNever()
                .HasColumnName("group_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.GroupName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("group_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
        });

        modelBuilder.Entity<EmployeeStatus>(entity =>
        {
            entity.HasKey(e => e.EmpStatusId).HasName("PK__Employee__8F2E59B90D6860EA");

            entity.ToTable("EmployeeStatus");

            entity.Property(e => e.EmpStatusId)
                .ValueGeneratedNever()
                .HasColumnName("emp_status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<EmployementType>(entity =>
        {
            entity.HasKey(e => e.EmployeeTypeId).HasName("PK__Employem__A36C7A1C6017A549");

            entity.ToTable("EmployementType");

            entity.Property(e => e.EmployeeTypeId).HasColumnName("employee_type_id");
            entity.Property(e => e.CreatedBy)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.TypeName)
                .IsUnicode(false)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.HolidayId).HasName("PK__Holidays__253884EA34BADC18");

            entity.Property(e => e.HolidayId).HasColumnName("holiday_id");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.FromDate)
                .HasColumnType("date")
                .HasColumnName("from_date");
            entity.Property(e => e.HolidayName).HasColumnName("holiday_name");
            entity.Property(e => e.ToDate)
                .HasColumnType("date")
                .HasColumnName("to_date");
        });

        modelBuilder.Entity<Industry>(entity =>
        {
            entity.HasKey(e => e.IndustryId).HasName("PK__Industry__A9676AC8181C3389");

            entity.ToTable("Industry");

            entity.Property(e => e.IndustryId)
                .ValueGeneratedNever()
                .HasColumnName("industry_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IndustryName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("industry_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__Jobs__6E32B6A50FD982C0");

            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ClosingDate)
                .HasColumnType("date")
                .HasColumnName("closing_date");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("department_name");
            entity.Property(e => e.Description)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.EmployeementType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("employeement_type");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.JobTittle)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Job_tittle");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.OfficeName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("office_name");
            entity.Property(e => e.ProbationEndDate)
                .HasColumnType("date")
                .HasColumnName("probation_end_date");
            entity.Property(e => e.ProbationStartDate)
                .HasColumnType("date")
                .HasColumnName("probation_start_date");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.FkCompany).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.FkCompanyId)
                .HasConstraintName("FK_Jobs_Company");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK_Jobs_Employee");
        });

        modelBuilder.Entity<JobHistory>(entity =>
        {
            entity.HasKey(e => e.JobhistoryId).HasName("PK__JobHisto__3A6F953611205635");

            entity.ToTable("JobHistory");

            entity.Property(e => e.JobhistoryId)
                .ValueGeneratedNever()
                .HasColumnName("jobhistory_id");
            entity.Property(e => e.EmploymentType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("employment_type");
            entity.Property(e => e.EndDate)
                .HasColumnType("date")
                .HasColumnName("end_date");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.FkJobId).HasColumnName("fk_job_id");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("job_title");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("note");
            entity.Property(e => e.Office)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("office");
            entity.Property(e => e.PositionType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("position_type");
            entity.Property(e => e.StartDate)
                .HasColumnType("date")
                .HasColumnName("start_date");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.JobHistories)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK_JobHistory_JobHistory");
        });

        modelBuilder.Entity<Leaf>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("PK__Leaves__743350BC7E1164A6");

            entity.Property(e => e.LeaveId)
                .ValueGeneratedNever()
                .HasColumnName("leave_id");
            entity.Property(e => e.ActualTotalLeaves).HasColumnName("actual_total_leaves");
            entity.Property(e => e.Attachment)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("attachment");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.FkLeaveTypeId).HasColumnName("fk_leave_type_id");
            entity.Property(e => e.FkScheduleId).HasColumnName("fk_schedule_id");
            entity.Property(e => e.FromDate)
                .HasColumnType("date")
                .HasColumnName("from_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("note");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.ToDate)
                .HasColumnType("date")
                .HasColumnName("to_date");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK__Leaves__fk_emplo__1B9317B3");

            entity.HasOne(d => d.FkLeaveType).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.FkLeaveTypeId)
                .HasConstraintName("FK__Leaves__fk_leave__1D7B6025");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.LeaveTypeId).HasName("PK__leave_ty__727714D4AB5334C5");

            entity.ToTable("leave_type");

            entity.Property(e => e.LeaveTypeId)
                .ValueGeneratedNever()
                .HasColumnName("leave_type_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Duration)
                .IsUnicode(false)
                .HasColumnName("duration");
            entity.Property(e => e.EligibleEmployeeType)
                .IsUnicode(false)
                .HasColumnName("eligible_employee_type");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("date")
                .HasColumnName("expiry_date");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.IsCarryForward).HasColumnName("is_carry_forward");
            entity.Property(e => e.IsPaid)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ispaid");
            entity.Property(e => e.Limit).HasColumnName("limit");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.SpecificEmployees)
                .IsUnicode(false)
                .HasColumnName("specific_employees");

            entity.HasOne(d => d.FkCompany).WithMany(p => p.LeaveTypes)
                .HasForeignKey(d => d.FkCompanyId)
                .HasConstraintName("FK_leave_type_Company");
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.LoginId).HasName("PK__LoginHis__C2C971DBF8021BA2");

            entity.ToTable("LoginHistory");

            entity.Property(e => e.LoginId)
                .ValueGeneratedNever()
                .HasColumnName("login_id");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.LoginDate)
                .HasColumnType("datetime")
                .HasColumnName("login_date");
            entity.Property(e => e.LoginStatus)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("login_status");
            entity.Property(e => e.LogoutDate)
                .HasColumnType("datetime")
                .HasColumnName("logout_date");
        });

        modelBuilder.Entity<MissingClockInOut>(entity =>
        {
            entity.HasKey(e => e.MissingclockinoutId).HasName("PK__MissingC__190B2EC9CAB71D29");

            entity.ToTable("MissingClockInOut");

            entity.Property(e => e.MissingclockinoutId)
                .ValueGeneratedNever()
                .HasColumnName("missingclockinout_id");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.MissingClockInTime).HasColumnName("missing_clock_in_time");
            entity.Property(e => e.MissingClockOutTime).HasColumnName("missing_clock_out_time");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.ModuleId).HasName("PK__Modules__2B7477870CE44DE4");

            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ModuleName)
                .HasMaxLength(255)
                .HasColumnName("module_name");
        });

        modelBuilder.Entity<Nationality>(entity =>
        {
            entity.HasKey(e => e.NationalityId).HasName("PK__National__F628E7A403D6D417");

            entity.ToTable("Nationality");

            entity.Property(e => e.NationalityId).HasColumnName("NationalityID");
            entity.Property(e => e.NationalityName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.NewsId).HasName("PK__News__954EBDD3319FC91F");

            entity.Property(e => e.NewsId).HasColumnName("news_id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.FkEmployeeId).HasColumnName("fk_employee_id");
            entity.Property(e => e.Imageurl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("imageurl");
            entity.Property(e => e.PublishedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("published_date");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.FkCompany).WithMany(p => p.News)
                .HasForeignKey(d => d.FkCompanyId)
                .HasConstraintName("FK_News_Company");

            entity.HasOne(d => d.FkEmployee).WithMany(p => p.News)
                .HasForeignKey(d => d.FkEmployeeId)
                .HasConstraintName("FK_News_Employee");
        });

        modelBuilder.Entity<OffLocation>(entity =>
        {
            entity.HasKey(e => e.OfficeLocationId).HasName("PK__OffLocat__C864FABF64C20A3C");

            entity.ToTable("OffLocation");

            entity.Property(e => e.OfficeLocationId).HasColumnName("office_location_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.Fkcompanyid).HasColumnName("fkcompanyid");
            entity.Property(e => e.Geofencing)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("geofencing");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Latitude1).HasColumnName("latitude1");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.Longitude1).HasColumnName("longitude1");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("modified_date");
            entity.Property(e => e.OfficeName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("office_name");
            entity.Property(e => e.Qrcode).HasColumnName("qrcode");
            entity.Property(e => e.Qrcodeimage)
                .IsUnicode(false)
                .HasColumnName("qrcodeimage");

            entity.HasOne(d => d.Fkcompany).WithMany(p => p.OffLocations)
                .HasForeignKey(d => d.Fkcompanyid)
                .HasConstraintName("FK_OffLocation_Company");
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(e => e.OfficeId).HasName("PK__Offices__2A196375281F3BCD");

            entity.Property(e => e.OfficeId).HasColumnName("office_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.ContactNo).HasColumnName("contact_no");
            entity.Property(e => e.Country)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("country");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Email)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("email");
            entity.Property(e => e.FkCityId).HasColumnName("fk_city_id");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.FkCountryId).HasColumnName("fk_country_id");
            entity.Property(e => e.FkOfficelocationId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("fk_officelocation_id");
            entity.Property(e => e.FkScheduleId).HasColumnName("fk_schedule_id");
            entity.Property(e => e.FkStateId).HasColumnName("fk_state_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.OfficeName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("office_name");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("postal_code");
            entity.Property(e => e.Qrcode)
                .IsUnicode(false)
                .HasColumnName("qrcode");
            entity.Property(e => e.Radius).HasColumnName("radius");
            entity.Property(e => e.State)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("state");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__E5331AFA2BDDFE49");

            entity.Property(e => e.PermissionId)
                .ValueGeneratedNever()
                .HasColumnName("permission_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.PermissionName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("permission_name");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.ReminderId).HasName("PK__Reminder__E27A3628D21485A2");

            entity.Property(e => e.ReminderId)
                .ValueGeneratedNever()
                .HasColumnName("reminder_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.ReminderDate)
                .HasColumnType("date")
                .HasColumnName("reminder_date");
            entity.Property(e => e.ReminderDetails)
                .HasColumnType("text")
                .HasColumnName("reminder_details");
            entity.Property(e => e.ReminderName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("reminder_name");
            entity.Property(e => e.ReminderTime).HasColumnName("reminder_time");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CCEB4BCDAD");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("role_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.RoleName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<RoleModulePermissionMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK__RoleModu__8B5781BD10CE3CB3");

            entity.ToTable("RoleModulePermissionMapping");

            entity.Property(e => e.MappingId).HasColumnName("mapping_id");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.PermissionFlag).HasColumnName("permission_flag");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleModulePermissionMappings)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__RoleModul__RoleI__6F4A8121");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PK__Sections__80EF0872C779387A");

            entity.Property(e => e.SectionId)
                .ValueGeneratedNever()
                .HasColumnName("section_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.SectionName)
                .HasMaxLength(255)
                .HasColumnName("section_name");
        });

        modelBuilder.Entity<SectionPermission>(entity =>
        {
            entity.HasKey(e => e.SectionPermissionId).HasName("PK__SectionP__9A304360DECD7AB9");

            entity.Property(e => e.SectionPermissionId)
                .ValueGeneratedNever()
                .HasColumnName("section_permission_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.PermissionFlag).HasColumnName("permission_flag");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");

            entity.HasOne(d => d.Company).WithMany(p => p.SectionPermissions)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_SectionPermissions_Company");

            entity.HasOne(d => d.Role).WithMany(p => p.SectionPermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SectionPe__RoleI__038683F8");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionPermissions)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SectionPe__Secti__047AA831");
        });

        modelBuilder.Entity<Stage>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("PK__Stages__03EB7AD812D4BA1A");

            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.StageName).HasMaxLength(255);

            entity.HasOne(d => d.FkCompany).WithMany(p => p.Stages)
                .HasForeignKey(d => d.FkCompanyId)
                .HasConstraintName("FK_Stages_Company");
        });

        modelBuilder.Entity<Statess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__statess__3213E83FE3DCA3CF");

            entity.ToTable("statess");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CountryId)
                .HasDefaultValueSql("((1))")
                .HasColumnName("country_id");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("name");

            entity.HasOne(d => d.Country).WithMany(p => p.Statesses)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_statess_countries");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__Subscrip__863A7EC12643307C");

            entity.ToTable("Subscription");

            entity.Property(e => e.SubscriptionId)
                .ValueGeneratedNever()
                .HasColumnName("subscription_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.SubscriptionName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("subscription_name");
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("PK__UserDevi__3B085D8BCAC5EBDF");

            entity.Property(e => e.DeviceId)
                .ValueGeneratedNever()
                .HasColumnName("device_id");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("device_name");
            entity.Property(e => e.DeviceToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("device_token");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("device_type");
            entity.Property(e => e.FkUserId).HasColumnName("fk_user_id");
        });

        modelBuilder.Entity<Workschedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Worksche__C46A8A6F0390EBCB");

            entity.ToTable("Workschedule");

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("date")
                .HasColumnName("created_date");
            entity.Property(e => e.DailyWorkingHours)
                .IsUnicode(false)
                .HasColumnName("daily_working_hours");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.FkCompanyId).HasColumnName("fk_company_id");
            entity.Property(e => e.HalfDayTime).HasColumnName("half_day_time");
            entity.Property(e => e.HoursPerDay).HasColumnName("hours_per_day");
            entity.Property(e => e.HoursPerWeek)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("hours_per_week");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
          /*  entity.Property(e => e.IsDefault).HasColumnName("is_default");*/
            entity.Property(e => e.LateTime).HasColumnName("late_time");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("date")
                .HasColumnName("modified_date");
            entity.Property(e => e.ScheduleName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("schedule_name");
            entity.Property(e => e.ScheduleType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("schedule_type");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.WorkingDays)
                .IsUnicode(false)
                .HasColumnName("working_days");

            entity.HasOne(d => d.FkCompany).WithMany(p => p.Workschedules)
                .HasForeignKey(d => d.FkCompanyId)
                .HasConstraintName("FK_Workschedule_Company");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
