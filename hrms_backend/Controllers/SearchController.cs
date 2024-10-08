using hrms_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext; // Replace YourDbContext with your actual DbContext class

        public SearchController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("Paging")]
        public IActionResult GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchTerm = "")
        {
            try
            {
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                if (page < 1)
                {
                    return BadRequest("Invalid page number. Page number must be greater than or equal to 1.");
                }

                if (pageSize < 1)
                {
                    return BadRequest("Invalid page size. Page size must be greater than or equal to 1.");
                }

                var employeesQuery = _dbContext.Employees
                    .Where(e => e.FkCompanyId == companyId)
                    .Include(e => e.FkDepartment)
                    .Include(e => e.FkDesignation)
                    .OrderBy(e => e.EmployeeId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        e.EmployeeId,
                        e.CreatedDate,
                        e.CreatedBy,
                        e.ModifiedDate,
                        e.ModifiedBy,
                        e.FkDepartmentId,
                        e.LineManagerId,
                        e.FkCompanyId,
                        e.FkUserId,
                        e.FirstName,
                        e.LastName,
                        e.MobileNo,
                        e.Email,

                        e.FullName,
                        e.Gender,
                        DateOfBirth = !string.IsNullOrEmpty(e.DateOfBirth) ? Convert.ToDateTime(e.DateOfBirth).ToString("dd MMM yyyy") : null,
                        e.MaritalStatus,
                        e.Nationality,
                        e.PersonalTaxId,
                        e.SocialInsurance,
                        e.HealthInsurance,
                        e.PhoneNumber,
                        e.JoinDate,
                        e.MarriageAnniversary,
                        e.AlternateMobileNo,
                        e.IsActive,
                        e.FkEmpstatusId,
                        CompanyName = e.FkCompany != null ? e.FkCompany.CompanyName : null,
                        OfficeName = e.FkOffice != null ? e.FkOffice.OfficeName : null,
                        EmployeeStatus = e.FkEmpstatus != null ? e.FkEmpstatus.StatusName : null,
                        FeaturedImageURL = e.Image,
                        e.FkLoginHistoryId,
                        e.FkOfficeId,
                        e.FkEmployeeGroupId,
                        DesignationName = e.FkDesignation != null ? e.FkDesignation.DesignationName : null,
                        DepartmentName = e.FkDepartment != null ? e.FkDepartment.DepartmentName : null,
                        // Include other properties as needed
                        LineManagerName = e.LineManagerId != null ? _dbContext.Employees
                            .Where(em => em.EmployeeId == e.LineManagerId)
                            .Select(em => em.FullName)
                            .FirstOrDefault() : null
                    });
                    
          

                // Apply filter based on the search term
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToUpper();
                    employeesQuery = employeesQuery.Where(e => e.FullName.ToUpper().Contains(searchTerm));
                }

                var totalRecords = _dbContext.Employees.Count(e => e.FkCompanyId == companyId);

                var response = new
                {
                    TotalRecords = totalRecords,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    Employees = employeesQuery.ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception for troubleshooting
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

    }
}
