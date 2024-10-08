using hrms_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class RolesController : ControllerBase
    {
        private readonly HrmsDbContext _context;

      
        public RolesController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpPost("createrolewithpermissions")]
        public async Task<IActionResult> CreateRoleWithPermissions([FromBody] RoleWithSectionPermissionsDTO dto)
        {
            try
            {
                var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

                // Check if EmployeeId is provided
                if (employeeIdClaim == null || string.IsNullOrEmpty(employeeIdClaim.Value))
                {
                    return BadRequest("EmployeeId is missing in the user claims.");
                }

                int createdBy = int.Parse(employeeIdClaim.Value);
                // Retrieve the CompanyGuid from the user's claims
                var companyGuidClaim = User.Claims.FirstOrDefault(c => c.Type == "company_guid");

                // Check if CompanyGuid is provided
                if (companyGuidClaim == null || string.IsNullOrEmpty(companyGuidClaim.Value))
                {
                    return BadRequest("CompanyGuid is missing in the user claims.");
                }

                // Resolve the companyId using the provided CompanyGuid
                var companyId = ResolveCompanyIdByGuid(companyGuidClaim.Value);

                if (companyId == 0)
                {
                    return NotFound("Company not found");
                }

                // Check if the role name already exists for the given company
                if (_context.Roles.Any(r => r.CompanyId == companyId && r.RoleName == dto.Role.RoleName))
                {
                    return BadRequest("Role with the same name already exists for the company.");
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        int nextRoleId = FindNextRoleId();
                        Role role = dto.Role;
                        role.RoleId = nextRoleId;
                        role.CompanyId = companyId; // Set the CompanyId

                        role.CreatedBy = employeeIdClaim.Value;
                        role.ModifiedBy = employeeIdClaim.Value;
                        role.IsActive = true;
                        role.ModifiedDate = DateTime.Now;
                        // Attach the role entity to start tracking it
                        _context.Roles.Add(role);
                        await _context.SaveChangesAsync();

                        var sectionPermissions = new List<SectionPermission>();

                        int nextSectionPermissionId = FindNextSectionPermissionId();

                        foreach (var sectionPermissionDto in dto.SectionPermissions)
                        {
                            var sectionPermission = new SectionPermission
                            {
                                SectionPermissionId = nextSectionPermissionId,
                                RoleId = role.RoleId,
                                SectionId = sectionPermissionDto.SectionId,
                                PermissionFlag = sectionPermissionDto.PermissionId,
                                CompanyId = companyId
                                
                            };

                            sectionPermissions.Add(sectionPermission);

                            nextSectionPermissionId++; // Increment SectionPermissionId for the next entry
                        }

                        _context.SectionPermissions.AddRange(sectionPermissions);
                        await _context.SaveChangesAsync();

                        // Commit the transaction
                        transaction.Commit();

                        // Retrieve permission names based on permission IDs
                        var permissionIds = dto.SectionPermissions.Select(p => p.PermissionId).ToList();
                        var permissionNames = GetPermissionNames(permissionIds);

                        return Ok(new
                        {
                            PermissionNames = permissionNames
                        });
                    }
                    catch (DbUpdateException ex)
                    {
                        // ... (Error handling)
                        transaction.Rollback();
                        return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException?.Message ?? ex.Message);
                    }
                    catch (Exception ex)
                    {
                        // ... (Error handling)
                        transaction.Rollback();
                        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
      

        private List<string> GetPermissionNames(List<int> permissionIds)
        {
            return _context.Permissions
                .Where(p => permissionIds.Contains(p.PermissionId))
                .Select(p => p.PermissionName)
                .ToList();
        }

        private int FindNextRoleId()
        {
            int nextRoleId = 1;
            if (_context.Roles.Any())
            {
                nextRoleId = _context.Roles.Max(r => r.RoleId) + 1;
            }
            return nextRoleId;
        }

        private int FindNextSectionPermissionId()
        {
            int nextSectionPermissionId = 1;
            if (_context.SectionPermissions.Any())
            {
                nextSectionPermissionId = _context.SectionPermissions.Max(sp => sp.SectionPermissionId) + 1;
            }
            return nextSectionPermissionId;
        }

    
        [HttpGet("getRoleWithMemberCount")]
        public IActionResult GetRolesWithEmployeeCount()
        {
            try
            {
                // Retrieve the CompanyGuid from the user's claims
                var companyGuidClaim = User.Claims.FirstOrDefault(c => c.Type == "company_guid");

                // Check if CompanyGuid is provided
                if (companyGuidClaim == null || string.IsNullOrEmpty(companyGuidClaim.Value))
                {
                    return BadRequest("CompanyGuid is missing in the user claims.");
                }

                // Resolve the companyId using the provided CompanyGuid
                var companyId = ResolveCompanyIdByGuid(companyGuidClaim.Value);

                if (companyId == 0)
                {
                    return NotFound("Company not found");
                }

                var rolesWithEmployeeCount = _context.Roles
                    .Where(role => role.CompanyId == companyId)  // Filter roles by companyId
                    .Select(role => new RoleWithEmployeeCountDTO
                    {
                        RoleId = role.RoleId,
                    
                        RoleName = role.RoleName,
                        EmployeeCount = _context.Employees.Count(employee => employee.fk_role_id == role.RoleId && employee.FkCompanyId == companyId)
                    })
                    .ToList();

                return Ok(rolesWithEmployeeCount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private int ResolveCompanyIdByGuid(string company_guid)
        {
            var company = _context.Companies.FirstOrDefault(c => c.CompanyGuid == Guid.Parse(company_guid));
            return company?.CompanyId ?? 0;
        }
        [HttpDelete("deleteRole/{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            try
            {
                // Retrieve the CompanyGuid from the user's claims
                var companyGuidClaim = User.Claims.FirstOrDefault(c => c.Type == "company_guid");

                // Check if CompanyGuid is provided
                if (companyGuidClaim == null || string.IsNullOrEmpty(companyGuidClaim.Value))
                {
                    return BadRequest("CompanyGuid is missing in the user claims.");
                }

                // Resolve the companyId using the provided CompanyGuid
                var companyId = ResolveCompanyIdByGuid(companyGuidClaim.Value);

                if (companyId == 0)
                {
                    return NotFound("Company not found");
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Retrieve the role to be deleted
                        var roleToDelete = await _context.Roles
                            .Include(r => r.SectionPermissions)  // Include related SectionPermissions
                            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.CompanyId == companyId);

                        if (roleToDelete == null)
                        {
                            return NotFound("Role not found");
                        }

                        // Remove associated SectionPermissions
                        _context.SectionPermissions.RemoveRange(roleToDelete.SectionPermissions);

                        // Set FKRoleId to null for employees with this role
                        var employeesWithRole = await _context.Employees
                            .Where(e => e.fk_role_id == roleId && e.FkCompanyId == companyId)
                            .ToListAsync();

                        foreach (var employee in employeesWithRole)
                        {
                            employee.fk_role_id = null;
                        }

                        // Remove the role
                        _context.Roles.Remove(roleToDelete);

                        // Delete role's permissions
                        var rolePermissions = await _context.RoleModulePermissionMappings
                            .Where(rp => rp.RoleId == roleId)
                            .ToListAsync();

                        _context.RoleModulePermissionMappings.RemoveRange(rolePermissions);

                        // Save changes
                        await _context.SaveChangesAsync();

                        // Commit the transaction
                        transaction.Commit();

                        return Ok("Role and associated permissions deleted successfully");
                    }
                    catch (DbUpdateException ex)
                    {
                        // ... (Error handling)
                        transaction.Rollback();
                        return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException?.Message ?? ex.Message);
                    }
                    catch (Exception ex)
                    {
                        // ... (Error handling)
                        transaction.Rollback();
                        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}

        
    
