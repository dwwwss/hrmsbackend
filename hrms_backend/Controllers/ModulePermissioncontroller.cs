using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using hrms_backend.Models;

[ApiController]
[Route("api/permissions")]
public class ModulePermissionsController : ControllerBase
{
    private readonly HrmsDbContext _context; // Replace YourDbContext with your actual DbContext class name

    public ModulePermissionsController(HrmsDbContext context)
    {
        _context = context;
    }

    [HttpPost("give")]
    public async Task<IActionResult> GivePermissions([FromBody] List<RoleModulePermissionMapping> permissionMappings)
    {
        try
        {
            foreach (var permissionMapping in permissionMappings)
            {
                // Check if there is already an existing mapping for the same RoleId and ModuleId
                var existingMapping = await _context.RoleModulePermissionMappings
                    .FirstOrDefaultAsync(m =>
                        m.RoleId == permissionMapping.RoleId &&
                        m.ModuleId == permissionMapping.ModuleId);

                if (existingMapping != null)
                {
                    // If PermissionId is different, update it
                    if (existingMapping.PermissionFlag != permissionMapping.PermissionFlag)
                    {
                        existingMapping.PermissionFlag = permissionMapping.PermissionFlag;
                        // Optionally, you can update other properties here if needed
                    }
                }
                else
                {
                    // If there is no existing mapping, create a new one
                    // Find the role
                    var role = await _context.Roles.FindAsync(permissionMapping.RoleId);

                    if (role == null)
                    {
                        return NotFound($"Role with ID {permissionMapping.RoleId} not found");
                    }

                    // Find the module
                    var module = await _context.Modules.FindAsync(permissionMapping.ModuleId);

                    if (module == null)
                    {
                        return NotFound($"Module with ID {permissionMapping.ModuleId} not found");
                    }

                    // Find or create the permission
                    var permission = await _context.Permissions
                        .FirstOrDefaultAsync(p => p.PermissionId == permissionMapping.PermissionFlag);

                    if (permission == null)
                    {
                        // Handle creating a new permission if needed
                    }

                    // Create the mapping
                    var newMapping = new RoleModulePermissionMapping
                    {
                        RoleId = role.RoleId,
                        ModuleId = module.ModuleId,
                        PermissionFlag = permission?.PermissionId, // Use existing or newly created permission
                                                                 // Set other properties as needed
                    };

                    _context.RoleModulePermissionMappings.Add(newMapping);
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Permissions granted successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }
    [HttpGet("modules")]
    public async Task<IActionResult> GetAllModules()
    {
        try
        {
            var modules = await _context.Modules
                .Select(m => new { m.ModuleId, m.ModuleName, m.Description })
                .ToListAsync();

            return Ok(modules);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // GET api/permissions/role/{roleId}/info
    [HttpGet("role/{roleId}/info")]
        public async Task<IActionResult> GetRoleInfo(int roleId)
        {
            try
            {
                // Find the role
                var role = await _context.Roles.FindAsync(roleId);

                if (role == null)
                {
                    return NotFound("Role not found");
                }

                // Find module names and associated permission IDs based on the role
                var modulePermissions = _context.RoleModulePermissionMappings
                    .Where(mapping => mapping.RoleId == roleId)
                    .Select(mapping => new
                    {
                        ModuleName = mapping.Module.ModuleName,
                        PermissionId = mapping.PermissionFlag,
                        moduleId=mapping.ModuleId
                    })
                    .ToList();

                return Ok(modulePermissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
    }

