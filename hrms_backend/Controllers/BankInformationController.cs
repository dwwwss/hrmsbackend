using Microsoft.AspNetCore.Mvc;
using System.Linq;
using hrms_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.VisualBasic;

namespace Hrms_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankInformationController : ControllerBase
    {
        private readonly HrmsDbContext _context;
        private readonly PermissionService _permissionService;

        public BankInformationController(HrmsDbContext dbContext, PermissionService permissionService)
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

            // Check if the user has the required permissions for the section
            if (!_permissionService.HasPermission(roleId, "Bank Information", 1) && !_permissionService.HasPermission(roleId, "Bank Information", 2))
            {
                // Return 401 Unauthorized if the user doesn't have the required permissions
                return Unauthorized("Unauthorized: Insufficient permissions");
            }

            // Create an anonymous object with only the desired address fields
            var bi = _context.BankInformations.FirstOrDefault(a => a.FkEmployeeId == employeeId);

            if (bi == null)
            {
                return NotFound(); // Address not found
            }

            // Create an anonymous object with only the desired address fields
            var result = new
            {
                bi.BankInfoId,
                bi.BankName,
                bi.AccountName,
                bi.Branch,
                bi.AccountNumber,
                bi.SwiftBic,
                bi.Iban,
                bi.IfscCode
            };

            return Ok(result);
        }
        [HttpPost("UpdateBankInformation/{employeeId}")]
        public IActionResult UpdateBankInformation(int employeeId, [FromBody] BankInformation updatedBankInfo)
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
            if (!_permissionService.HasPermission(roleId, "Bank Information", 2))
            {
                // Return 401 Unauthorized if the user doesn't have the required permissions
                return Unauthorized("Unauthorized: Insufficient permissions");
            }

            var bankInformation = _context.BankInformations.FirstOrDefault(bi => bi.FkEmployeeId == employeeId);

            if (bankInformation == null)
            {
                return NotFound(); // Bank information not found
            }

            // Check if required fields (BankName, AccountName, AccountNumber, and Iban) are present
            if (string.IsNullOrWhiteSpace(updatedBankInfo.BankName) ||
                string.IsNullOrWhiteSpace(updatedBankInfo.AccountName) ||
                string.IsNullOrWhiteSpace(updatedBankInfo.AccountNumber) ||
                string.IsNullOrWhiteSpace(updatedBankInfo.Iban))
            {
                return BadRequest("Bank Name, Account Name, Account Number, and Iban are required fields.");
            }

            try
            {
                // Update the bank information with the provided values
                bankInformation.BankName = updatedBankInfo.BankName;
                bankInformation.AccountName = updatedBankInfo.AccountName;
                bankInformation.Branch = updatedBankInfo.Branch;
                bankInformation.AccountNumber = updatedBankInfo.AccountNumber;
                bankInformation.SwiftBic = updatedBankInfo.SwiftBic;
                bankInformation.Iban = updatedBankInfo.Iban;
                bankInformation.IfscCode = updatedBankInfo.IfscCode;

                _context.SaveChanges();

                return Ok("Bank information updated successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception or return an appropriate error response
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the bank information.");
            }
        }

    }
}
