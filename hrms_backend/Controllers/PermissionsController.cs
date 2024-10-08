using hrms_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly HrmsDbContext _context;
        public PermissionsController(HrmsDbContext context)
        {
            _context = context;
        }
        public class SectionPermissionDto
        {
            public string SectionName { get; set; }
            public string PermissionName { get; set; }
        }

        [HttpGet("getsectionsandpermissionsforrole/{roleId}")]
        public IActionResult GetSectionsAndPermissionsForRole(int roleId)
        {
            var role = _context.Roles
                .Include(r => r.SectionPermissions)
                .ThenInclude(sp => sp.Section)
                .ThenInclude(s => s.SectionPermissions)
                .FirstOrDefault(r => r.RoleId == roleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }
            var sectionPermissions = role.SectionPermissions
                .Where(sp => sp.Section != null)
                .Select(sp => new SectionPermissionDto
                {
                    SectionName = sp.Section.SectionName,
                    PermissionName = GetPermissionName(sp.PermissionFlag)
                })
                .ToList();
            var result = new
            {
                RoleName = role.RoleName,
                Sections = sectionPermissions
            };
            return Ok(result);
        }
        private string GetPermissionName(int permissionId)
        {
            var permission = _context.Permissions
                .FirstOrDefault(p => p.PermissionId == permissionId);
            if (permission != null)
            {
                return permission.PermissionName;
            }
            // Handle cases where no permission is found
            return "No permission found";
        }
        [HttpGet("getsection")]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSections()
        {
            var sections = await _context.Sections
                .Select(s => new SectionDto
                {
                    SectionId = s.SectionId,
                    SectionName = s.SectionName
                })
                .ToListAsync();

            return Ok(sections);
        }

        public class SectionDto
        {
            public int SectionId { get; set; }
            public string SectionName { get; set; }
        }
        [HttpPost("editrolepermissions")]
        public async Task<IActionResult> EditRolePermissions([FromBody] RoleWithSectionPermissionsDTO dto)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Role existingRole = _context.Roles.FirstOrDefault(r => r.RoleId == dto.Role.RoleId);

                    if (existingRole == null)
                    {
                        return BadRequest("Role not found.");
                    }

                    // Check if the role is editable
                    if (!IsRoleEditable(existingRole.RoleName))
                    {
                        return BadRequest($"Permissions are not editable for the role '{existingRole.RoleName}'.");
                    }

                    foreach (var sectionPermissionDto in dto.SectionPermissions)
                    {
                        var existingPermission = _context.SectionPermissions
                            .FirstOrDefault(sp =>
                                sp.RoleId == existingRole.RoleId &&
                                sp.SectionId == sectionPermissionDto.SectionId);

                        if (existingPermission != null)
                        {
                            // Update existing permission
                            existingPermission.PermissionFlag = sectionPermissionDto.PermissionId;
                        }
                        else
                        {
                            // Section permission doesn't exist, return an error
                            transaction.Rollback();
                            return BadRequest($"Section permission for SectionId {sectionPermissionDto.SectionId} not found for the given role.");
                        }
                    }

                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    transaction.Commit();

                    // Return the updated role and permissions
                    var updatedRole = await _context.Roles
                        .Include(r => r.SectionPermissions)
                        .ThenInclude(sp => sp.Section)
                        .ThenInclude(s => s.SectionPermissions)
                        .FirstOrDefaultAsync(r => r.RoleId == existingRole.RoleId);

                    if (updatedRole == null)
                    {
                        return BadRequest("Role not found after update.");
                    }

                    // Extract section names, and related permissions with names
                    var sectionNames = updatedRole.SectionPermissions.Select(sp => sp.Section.SectionName).ToList();
                    var permissionIds = dto.SectionPermissions.Select(p => p.PermissionId).ToList();

                    return Ok(new
                    {
                        Message = "Role permissions updated successfully.",
                    });
                }
                catch (DbUpdateException ex)
                {
                    var innerException = ex.InnerException;
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError, innerException?.Message ?? ex.Message);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                }
            }
        }

        // Helper method to check if a role is editable
        private bool IsRoleEditable(string roleName)
        {
            // Add conditions here based on role names that are not editable
            return !string.Equals(roleName, "Superadmin", StringComparison.OrdinalIgnoreCase);
        }
    }
    }




        

        
