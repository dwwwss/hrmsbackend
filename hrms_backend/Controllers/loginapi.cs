using hrms_backend.Models;
using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System.Security.Cryptography;
using System.Threading.Tasks;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Initialize the database context and configuration for this controller
        private readonly HrmsDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(HrmsDbContext context, IConfiguration configuration)
        {
            // Dependency injection: Store the database context and configuration for later use
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Employee loginModel)
        {
            try
            {
                // Authenticate the user based on the provided loginModel (username and password)
                var employee = await _context.Employees
                    .Join(
                        _context.Roles,
                        e => e.fk_role_id,
                        r => r.RoleId,
                        (e, r) => new { Employee = e, Role = r }
                    )
                    .Join(
                        _context.Companies,
                        e => e.Employee.FkCompanyId,
                        c => c.CompanyId,
                        (e, c) => new { Employee = e.Employee, Role = e.Role, Company = c }
                    )
                    .FirstOrDefaultAsync(e => e.Employee.Email == loginModel.Email && e.Employee.Password == loginModel.Password);

                Console.WriteLine($"ResetToken in the database: {employee?.Employee?.ResetToken}");

                if (employee != null)
                {
                    var roleId = employee.Role.RoleId;
                    var roleName = employee.Role.RoleName;

                    // Log resetToken for debugging
                    Console.WriteLine($"ResetToken in the response: {employee.Employee.ResetToken}");

                    if (roleName == "Superadmin")
                    {
                        // Check if the employee is verified for Superadmin
                        if (employee.Employee.IsVerified.HasValue && !employee.Employee.IsVerified.Value)
                        {
                            return BadRequest("Account not verified. Please verify your email first.");
                        }
                    }

                    // Check if the activation token is null or empty
                    if (!string.IsNullOrEmpty(employee.Employee.ActivationToken))
                    {
                        return BadRequest("Account not activated. Please activate your account first.");
                    }

                    // Extract user information and company details for the response
                    var id = employee.Employee.EmployeeId;
                    var email = employee.Employee.Email;
                    var name = employee.Employee.FullName;
                    var companyId = employee.Company.CompanyId;
                    var companyName = employee.Company.CompanyName;
                    var companyGuid = employee.Company.CompanyGuid;
                    var officeid = employee.Employee.FkOfficeId;


                    // Simplified permissions
                    var permissions = GetPermissionsForUser(roleId);
                    var modulePermissions = GetModulePermissionsForUser(roleId);

                    // Add module permissions to claims

                    // Generate JWT claims and token
                    var claims = new List<Claim>
            {
                new Claim("EmployeeId", id.ToString()),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("CompanyId", companyId.ToString()),
                new Claim("roleId", roleId.ToString()),
                new Claim("CompanyName", companyName),
                new Claim("company_guid", companyGuid.ToString()),
                new Claim("permissions", companyGuid.ToString()),
                new Claim("email", email.ToString())
            };

                    // Add ResetToken to claims
                    claims.Add(new Claim("ResetToken", employee.Employee.ResetToken ?? "0"));

                    claims.AddRange(modulePermissions
                        .Select(permission => new Claim($"ModulePermission_{permission.ModuleId}", permission.PermissionId.ToString()))
                    );

                    // Add permissions to claims
                    claims.AddRange(permissions
                        .Select(permission => new Claim($"Permission_{permission.SectionId}", permission.PermissionId.ToString()))
                    );

                    var token = GenerateJwtToken(claims);

                    return Ok(new
                    {
                        id,
                        token,
                        email,
                        name,
                        roleId,
                        officeid,
                        roleName,
                        companyId,
                        companyGuid,
                        companyName,
                        permissions,
                        modulePermissions,
                        ResetToken = employee.Employee.ResetToken ?? "Null" // Set to "0" if null
                    });
                }

                // Return an error response if authentication fails
                return BadRequest("Invalid email or password.");
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        // Generate a JWT token based on the provided claims
        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private List<ModulePermissionDTO> GetModulePermissionsForUser(int roleId)
        {
            var modulePermissions = _context.RoleModulePermissionMappings
                .Where(p => p.RoleId == roleId)
                .Select(p => new ModulePermissionDTO
                {
                    ModuleId = (int)p.ModuleId,
                    PermissionId = (int)p.PermissionFlag,
                })
                .ToList();

            return modulePermissions;
        }

        private List<SectionPermissionDTO> GetPermissionsForUser(int roleId)
        {
            var permissions = _context.SectionPermissions
                .Where(p => p.RoleId == roleId)
                .Select(p => new SectionPermissionDTO
                {
                    SectionId = p.SectionId,
                    PermissionId = p.PermissionFlag,
                })
                .ToList();

            return permissions;
        }

        // A utility class to generate a random JWT key
        public static class JwtKeyGenerator
        {
            public static string GenerateJwtKey(int keySizeInBytes = 32)
            {
                using (var cryptoProvider = new RNGCryptoServiceProvider())
                {
                    byte[] secretKeyBytes = new byte[keySizeInBytes];
                    cryptoProvider.GetBytes(secretKeyBytes);
                    return Convert.ToBase64String(secretKeyBytes);
                }
            }
        }
        public class TokenValidationService
        {
            private readonly BlacklistService _blacklistService;

            public TokenValidationService(BlacklistService blacklistService)
            {
                _blacklistService = blacklistService;
            }

            public bool ValidateLifetime(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
            {
                if (securityToken is JwtSecurityToken jwtSecurityToken)
                {
                    // Check if the token is in the blacklist
                    if (_blacklistService.IsTokenBlacklisted(jwtSecurityToken.RawData))
                    {
                        return false; // Token is blacklisted
                    }
                }

                return true; // Token is valid
            }
        }
        [HttpPost("logout")]
        public IActionResult Logout([FromServices] BlacklistService blacklistService)
        {
            try
            {
                // Retrieve the current user's identity
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                // Retrieve the user's email from the claims
                var userEmail = identity?.FindFirst(ClaimTypes.Name)?.Value;

                // Retrieve the JWT token from the request header
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Check if the token is in the blacklist
                if (blacklistService.IsTokenBlacklisted(token))
                {
                    return BadRequest(new { Message = "Token already invalidated" });
                }

                // Add the token to the blacklist
                blacklistService.AddToBlacklist(token);

                return Ok(new { Message = "Logout successful", Email = userEmail });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "An error occurred during logout", Error = ex.Message });
            }
        }


    }
}
