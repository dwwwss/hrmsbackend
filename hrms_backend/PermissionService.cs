using hrms_backend.Models;
using Microsoft.EntityFrameworkCore;

public class PermissionService
{
    private readonly HrmsDbContext _context;

    public PermissionService(HrmsDbContext context)
    {
        _context = context;
    }

    public bool HasPermission(int roleId, string sectionName, int requiredPermissionId)
    {
        // Log roleId, sectionName, and requiredPermissionId for debugging
        Console.WriteLine($"Checking permission for roleId: {roleId}, sectionName: {sectionName}, permissionId: {requiredPermissionId}");

        // Logic to check if the roleId has the requiredPermissionId for the given sectionName
        var sectionPermission = _context.SectionPermissions
            .Include(sp => sp.Section) // Assuming SectionPermission has a navigation property 'Section'
            .FirstOrDefault(sp =>
                sp.RoleId == roleId &&
                sp.Section.SectionName == sectionName &&
                sp.PermissionFlag == requiredPermissionId);

        // Log the result for debugging
        Console.WriteLine($"Permission result: {sectionPermission != null}");

        // Return true if authorized, false otherwise
        return sectionPermission != null;
    }

}
