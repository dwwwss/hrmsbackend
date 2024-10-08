using hrms_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext; // Replace YourDbContext with your actual DbContext class
        public JobsController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        // GET: api/Jobs
        [HttpGet]
        public IActionResult GetJobs()
        {
            // Get the company ID from the user's claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            if (companyIdClaim == null || string.IsNullOrEmpty(companyIdClaim.Value))
            {
                // Handle the case where the company ID claim is missing or empty
                return BadRequest("Company ID claim is missing or invalid.");
            }
            // Parse the company ID from the claim 
            int companyId;
            if (!int.TryParse(companyIdClaim.Value, out companyId))
            {
                // Handle the case where the company ID cannot be parsed as an integer
                return BadRequest("Invalid format for Company ID.");
            }
            // Get jobs for the specified company
            var jobs = _dbContext.Jobs
                .Where(j => j.FkCompanyId == companyId) // Assuming CompanyId is a property in the Jobs model
                .Select(j => new
                {
                    Job = j,
                    CandidateCount = _dbContext.Candidates.Count(c => c.FkJobId == j.JobId)
                })
                .ToList();
            return Ok(jobs);
        }
        // GET: api/Jobs/5
        [HttpGet("{id}")]
        public IActionResult GetJob(int id)
        {
            var job = _dbContext.Jobs.Find(id);
            if (job == null)
            {
                return NotFound();
            }
            return Ok(job);
        }
        // POST: api/Jobs
        [HttpPost]
        public IActionResult PostJob([FromBody] Job job)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Retrieve the FkCompanyId from the claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int CompanyId))
            {
                // Handle the case where company information is not available in the token
                return BadRequest("Invalid data. Company information is missing.");
            }
            // Set the FkCompanyId in the job object
            job.FkCompanyId = CompanyId;
            job.CreatedDate = DateTime.Now.Date;
            job.ModifiedDate = DateTime.Now.Date;
            job.IsActive = true;
            _dbContext.Jobs.Add(job);
            _dbContext.SaveChanges();
            return CreatedAtAction("GetJob", new { id = job.JobId }, job);
        }
        [HttpPut("{id}")]
        public IActionResult PutJob(int id, [FromBody] Job updatedJob)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Retrieve the company ID from the claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                // Handle the case where company information is not available in the token
                return BadRequest("Invalid data. Company information is missing.");
            }
            // Ensure that the job being updated belongs to the company
            var existingJob = _dbContext.Jobs.FirstOrDefault(j => j.JobId == id && j.FkCompanyId == companyId);
            if (existingJob == null)
            {
                // Handle the case where the job does not exist or does not belong to the company
                return NotFound("Job not found or does not belong to the company.");
            }
            // Update job properties
            existingJob.JobTittle = updatedJob.JobTittle;
            existingJob.Description = updatedJob.Description;
            existingJob.EmployeementType = updatedJob.EmployeementType;
            existingJob.DepartmentName = updatedJob.DepartmentName;
            existingJob.OfficeName = updatedJob.OfficeName;
            existingJob.Quantity = updatedJob.Quantity;
            existingJob.ClosingDate = updatedJob.ClosingDate;
            // Update other properties as needed
            existingJob.ModifiedDate = DateTime.Now;
            _dbContext.SaveChanges();
            return Ok(existingJob);
        }
        [HttpGet("departmentlist")]
        public IActionResult GetAllDepartments()
        {
            // Retrieve all departments from the database
            var departments = _dbContext.Departments
                .Select(d => new
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName
                })
                .ToList();

            // Return the department information as a response
            return Ok(departments);
        }
        [HttpGet("designationlist")]
        public IActionResult GetAllDesignation()
        {
            // Retrieve all departments from the database
            var designations = _dbContext.Designations
                .Select(d => new
                {
                    DepartmentId = d.DesignationId,
                    DepartmentName = d.DesignationName
                })
                .ToList();

            // Return the department information as a response
            return Ok(designations);
        }

        // DELETE: api/Jobs/5
        [HttpDelete("{id}")]
        public IActionResult DeleteJob(int id)
        {
            // Retrieve the company ID from the claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                // Handle the case where company information is not available in the token
                return BadRequest("Invalid data. Company information is missing.");
            }

            // Ensure that the job being deleted belongs to the company
            var jobToDelete = _dbContext.Jobs.Include(j => j.Candidates).FirstOrDefault(j => j.JobId == id && j.FkCompanyId == companyId);
            if (jobToDelete == null)
            {
                // Handle the case where the job does not exist or does not belong to the company
                return NotFound("Job not found or does not belong to the company.");
            }

            // Check if the job has associated candidates
            if (jobToDelete.Candidates.Any())
            {
                // If there are associated candidates, delete them first
                _dbContext.Candidates.RemoveRange(jobToDelete.Candidates);
            }

            // Now, delete the job
            _dbContext.Jobs.Remove(jobToDelete);
            _dbContext.SaveChanges();

            return Ok($"Job with ID {id} and its associated candidates have been deleted.");
        }

    }
}