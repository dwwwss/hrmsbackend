using hrms_backend.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly PermissionService _permissionService;

        public AddressController(HrmsDbContext dbContext, PermissionService permissionService)
        {
            _context = dbContext;
            _permissionService = permissionService;
        }

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

            // Get the section name for the role
       

            // Check if the user has the required permissions for the section
            if (!_permissionService.HasPermission(roleId, "Address", 1) &&
                !_permissionService.HasPermission(roleId, "Address", 2))
            {
                // Return 401 Unauthorized if the user doesn't have the required permissions
                return Unauthorized("Unauthorized: Insufficient permissions");
            }

            // Retrieve the address based on the employeeId
            var address = _context.Addresses.FirstOrDefault(a => a.FkEmployeeId == employeeId);

            if (address == null)
            {
                return NotFound(); // Address not found
            }

            // Create an anonymous object with only the desired address fields
            var result = new
            {
                address.AddressId,
                address.PrimaryAddress,
                address.Country,
                address.City,
                address.StateProvince,
                address.PostalCode,
                address.PermanentAddress
            };

            return Ok(result);
        }

        [HttpPost("AddToEmployee/{employeeId}")]
        public IActionResult CreateOrUpdateAddressForEmployee(int employeeId, [FromBody] Address address)
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

            if (!_permissionService.HasPermission(roleId, "Address", 2))
            {
                // Return 401 Unauthorized if the user doesn't have the required permissions
                return Unauthorized("Unauthorized: Insufficient permissions");
            }
            if (address == null)
            {
                return BadRequest("Invalid data. Address object is null.");
            }

            try
            {
                // Check if the address already exists for the specified employee
                var existingAddress = _context.Addresses.FirstOrDefault(a => a.FkEmployeeId == employeeId);

                if (existingAddress == null)
                {
                    // If the address does not exist, create a new one and associate it with the employee
                    // Manually increment the AddressId
                    int maxAddressId = _context.Addresses.Max(a => a.AddressId);
                    address.AddressId = maxAddressId + 1;
                    address.FkEmployeeId = employeeId;
                    _context.Addresses.Add(address);
                }
                else
                {
                    // If the address already exists, update its properties
                    existingAddress.PrimaryAddress = address.PrimaryAddress;
                    existingAddress.Country = address.Country;
                    existingAddress.City = address.City;
                    existingAddress.StateProvince = address.StateProvince;
                    existingAddress.PostalCode = address.PostalCode;
                    existingAddress.PermanentAddress = address.PermanentAddress;
                    _context.Addresses.Update(existingAddress);
                }
                // You can also perform other validation and business logic here before saving the address
                _context.SaveChanges();

                return Ok("Address updated successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception or return an appropriate error response
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the address.");
            }
        }
    }
}
