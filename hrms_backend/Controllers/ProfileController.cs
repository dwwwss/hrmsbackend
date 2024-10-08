using hrms_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Linq;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;
        private readonly PermissionService _permissionService;

        public ProfileController(HrmsDbContext dbContext, PermissionService permissionService)
        {
            _dbContext = dbContext;
            _permissionService = permissionService;
        }

        // GET api/Profile/{id}
        [HttpGet("{id}")]
        public IActionResult GetProfile(int id)
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
            if (!_permissionService.HasPermission(roleId, "Personal Information", 1) && !_permissionService.HasPermission(roleId, "Personal Information", 2))
            {
                // Return 401 Unauthorized if the user doesn't have the required permissions
                return Unauthorized("Unauthorized: Insufficient permissions");
            }

            var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == id);
            if (employee == null)
            {
                return NotFound(); // Employee not found
            }

            // Create an anonymous object with the desired profile fields
            var profile = new
            {
                FullName = employee.FullName,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth,
                MaritalStatus = employee.MaritalStatus,
                Nationality = employee.Nationality,
                PersonalTaxId = employee.PersonalTaxId,
                Email = employee.Email,
                SocialInsurance = employee.SocialInsurance,
                HealthInsurance = employee.HealthInsurance,
                PhoneNumber = employee.PhoneNumber,
                MarriageAnniversary = employee.MarriageAnniversary,
                PersonalEmail = employee.Email,
                AlternateMobileNo = employee.AlternateMobileNo
            };

            return Ok(profile);
        }

        // POST api/Profile/{id}
        [HttpPost("{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] Employee updatedEmployee)
        {
            // Get the role id claim from the token
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
            if (!_permissionService.HasPermission(roleId, "Personal Information", 2))
            {
                // Return 401 Unauthorized if the user doesn't have the required permissions
                return Unauthorized("Unauthorized: Insufficient permissions");
            }

            var employee = _dbContext.Employees.FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound(); // Employee not found
            }

            // Check if required fields (FullName and Email) are present in the updatedEmployee
            if (string.IsNullOrWhiteSpace(updatedEmployee.FullName) || string.IsNullOrWhiteSpace(updatedEmployee.Email))
            {
                return BadRequest("Full Name and Email are required fields.");
            }

            try
            {
                // Update the profile fields with the provided values
                employee.FullName = updatedEmployee.FullName;
                employee.Gender = updatedEmployee.Gender;
                employee.DateOfBirth = updatedEmployee.DateOfBirth;
                employee.MaritalStatus = updatedEmployee.MaritalStatus;
                employee.Nationality = updatedEmployee.Nationality;
                employee.PersonalTaxId = updatedEmployee.PersonalTaxId;
                employee.Email = updatedEmployee.Email;
                employee.SocialInsurance = updatedEmployee.SocialInsurance;
                employee.HealthInsurance = updatedEmployee.HealthInsurance;
                employee.PhoneNumber = updatedEmployee.PhoneNumber;
                employee.MarriageAnniversary = updatedEmployee.MarriageAnniversary;
                employee.AlternateMobileNo = updatedEmployee.AlternateMobileNo;

                _dbContext.SaveChanges();

                return Ok("Profile updated successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception or return an appropriate error response
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the profile.");
            }
        }

    }
}

