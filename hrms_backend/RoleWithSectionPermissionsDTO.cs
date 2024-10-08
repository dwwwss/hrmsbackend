using hrms_backend.Models;

public class RoleWithSectionPermissionsDTO
{
    public  Role Role { get; set; }
    public List<SectionPermissionDTO> SectionPermissions { get; set; }
    public Guid CompanyGuid { get; set; }
}

public class SectionPermissionDTO
{
    public int SectionId { get; set; }
    public int PermissionId { get; set; }
    public int? CompanyId { get; set; }
  
  
}
