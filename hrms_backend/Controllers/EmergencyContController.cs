using hrms_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyContController : ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly PermissionService _permissionService;

        public EmergencyContController(HrmsDbContext dbContext, PermissionService permissionService)
        {
            _context = dbContext;
            _permissionService = permissionService;
        }

        // GET: api/EmergencyContact/{id}
        [HttpGet("{employeeId}")]
        public IActionResult GetAddress(int employeeId)
        {
            var roleIdClaim = User.Claims.FirstOrDefault(c => c.Type == "roleId");

            if (roleIdClaim == null)
            {
                // Return 401 Unauthorized if the necessary claims are not present
                return Unauthorized("Unauthorized: Insufficient claims");
            }

            // Extract role id from the claim
            int roleId;
            if (!int.TryParse(roleIdClaim.Value, out roleId))
            {
                // Return 401 Unauthorized if roleId claim is not a valid integer
                return Unauthorized("Unauthorized: Invalid role id claim");
            }

            // Check if the user has the required permissions for the section
            if (!_permissionService.HasPermission(roleId, "Emergency Contact", 1) && !_permissionService.HasPermission(roleId, "Emergency Contact", 2))
            {
                // Return 401 Unauthorized if the user doesn't have the required permissions
                return Unauthorized("Unauthorized: Insufficient permissions");
            }

            // Retrieve the address based on the employeeId
            var emergencycontact = _context.EmergencyContacts.FirstOrDefault(a => a.FkEmployeeId == employeeId);

            if (emergencycontact == null)
            {
                return NotFound(); // Address not found
            }

            // Create an anonymous object with only the desired address fields
            var result = new
            {
                emergencycontact.EmergencyContactId,
                emergencycontact.FullName,
                emergencycontact.Relationship,
                emergencycontact.PhoneNumber

            };

            return Ok(result);
        }
        [HttpPost("AddToEmployee/{employeeId}")]
        public IActionResult CreateOrUpdateEmergencyContactForEmployee(int employeeId, [FromBody] EmergencyContact emergencyContact)
        {
            var roleIdClaim = User.Claims.FirstOrDefault(c => c.Type == "roleId");

            if (roleIdClaim == null)
            {
                // Return 401 Unauthorized if the necessary claims are not present
                return Unauthorized("Unauthorized: Insufficient claims");
            }

            // Extract role id from the claim
            int roleId;
            if (!int.TryParse(roleIdClaim.Value, out roleId))
            {
                // Return 401 Unauthorized if roleId claim is not a valid integer
                return Unauthorized("Unauthorized: Invalid role id claim");
            }

            // Check if the user has the required permissions for the section

            if (!_permissionService.HasPermission(roleId, "Emergency Contact", 2))
            {
                // Return 401 Unauthorized if the user doesn't have the required permissions
                return Unauthorized("Unauthorized: Insufficient permissions");
            }
            if (emergencyContact == null)
            {
                return BadRequest("Invalid data. EmergencyContact object is null.");
            }
            // Validate the phone number format
            if (!IsValidPhoneNumber(emergencyContact.PhoneNumber))
            {
                return BadRequest("Invalid phone number format. Please provide a valid phone number.");
            }
            try
            {
                // Retrieve the employee based on the employeeId
                var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
                if (employee == null)
                {
                    return BadRequest("Invalid EmployeeId. Employee not found.");
                }

                // Get the current maximum EmergencyContactId from the table
                int maxEmergencyContactId = _context.EmergencyContacts.Max(ec => ec.EmergencyContactId);

                // If the table is truncated, assume EmergencyContactId starts from 0
                int nextEmergencyContactId = maxEmergencyContactId + 1;

                // Check if the emergency contact exists for the specified employee
                var existingEmergencyContact = _context.EmergencyContacts.FirstOrDefault(ec => ec.FkEmployeeId == employeeId);
                if (existingEmergencyContact == null)
                {
                    // If the emergency contact does not exist, create a new one and associate it with the employee
                    emergencyContact.EmergencyContactId = nextEmergencyContactId;
                    emergencyContact.FkEmployeeId = employeeId;
                    _context.EmergencyContacts.Add(emergencyContact);
                }
                else
                {
                    // If the emergency contact already exists, update its properties
                    existingEmergencyContact.FullName = emergencyContact.FullName;
                    existingEmergencyContact.Relationship = emergencyContact.Relationship;
                    existingEmergencyContact.PhoneNumber = emergencyContact.PhoneNumber;
                    _context.EmergencyContacts.Update(existingEmergencyContact);
                }

                _context.SaveChanges();

                return Ok("Emergency contact updated successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception or return an appropriate error response
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating/updating the emergency contact.");
            }
        }
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Validate phone number format (for example, allowing only digits and dashes)
            string pattern = @"^[0-9-]+$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }
}
