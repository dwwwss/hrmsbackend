using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hrms_backend.Models;
using System.ComponentModel.Design;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplateController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;

        public EmailTemplateController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public ActionResult<IEnumerable<EmailTemplateDto>> GetEmailTemplates()
        {
            // Extract companyId from claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            // Check if the companyId claim exists
            if (companyIdClaim == null)
            {
                // Handle the case where companyId claim is missing
                return BadRequest("Company ID claim not found");
            }

            // Convert the companyId claim value to an integer
            if (!int.TryParse(companyIdClaim.Value, out int CompanyId))
            {
                // Handle the case where companyId claim is not a valid integer
                return BadRequest("Invalid Company ID");
            }

            var emailTemplates = _dbContext.EmailTemplates
                .Where(et => et.FkCompanyId == CompanyId)
                .Select(et => new EmailTemplateDto
                {
                    TemplateId = et.TemplateId,
                    Subject = et.Subject,
                    Stage = et.Stage,
                    LastModified = et.LastModified.HasValue ? et.LastModified.Value.Date.ToString("dd MMM yyyy") : null,
                    Body = et.Body,
                    FkCompanyId = et.FkCompanyId,
                    EmailTemplate1 = et.EmailTemplate1
                })
                .ToList();

            return Ok(emailTemplates);
        }

        public class EmailTemplateDto
        {
            public int TemplateId { get; set; }
            public string Subject { get; set; }
            public string Stage { get; set; }
            public string LastModified { get; set; } // Change the type to string
            public string Body { get; set; }
            public int? FkCompanyId { get; set; }
            public string EmailTemplate1 { get; set; }
        }

        [HttpGet("{id}")]
        public ActionResult<EmailTemplateDto> GetEmailTemplate(int id)
        {
            // Extract companyId from claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            // Check if the companyId claim exists
            if (companyIdClaim == null)
            {
                // Handle the case where companyId claim is missing
                return BadRequest("Company ID claim not found");
            }

            // Convert the companyId claim value to an integer
            if (!int.TryParse(companyIdClaim.Value, out int CompanyId))
            {
                // Handle the case where companyId claim is not a valid integer
                return BadRequest("Invalid Company ID");
            }

            // Retrieve email template based on companyId, specific date, and id
            var emailTemplate = _dbContext.EmailTemplates
                .FirstOrDefault(et => et.TemplateId == id && et.FkCompanyId == CompanyId);

            if (emailTemplate == null)
            {
                return NotFound();
            }

            // Convert the result to EmailTemplateDto for consistent formatting
            var emailTemplateDto = new EmailTemplateDto
            {
                TemplateId = emailTemplate.TemplateId,
                Subject = emailTemplate.Subject,
                Stage = emailTemplate.Stage,
                LastModified = emailTemplate.LastModified.HasValue ? emailTemplate.LastModified.Value.Date.ToString("dd MMM yyyy") : null,
                Body = emailTemplate.Body,
                FkCompanyId = emailTemplate.FkCompanyId,
                EmailTemplate1 = emailTemplate.EmailTemplate1
            };

            return Ok(emailTemplateDto);
        }
     
        // PUT: api/EmailTemplate/5
        [HttpPut("{id}")]
        public IActionResult PutEmailTemplate(int id, [FromBody] EmailTemplate updatedEmailTemplateDto)
        {
            // Extract companyId from claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            // Check if the companyId claim exists
            if (companyIdClaim == null)
            {
                // Handle the case where companyId claim is missing
                return BadRequest("Company ID claim not found");
            }

            // Convert the companyId claim value to an integer
            if (!int.TryParse(companyIdClaim.Value, out int CompanyId))
            {
                // Handle the case where companyId claim is not a valid integer
                return BadRequest("Invalid Company ID");
            }

            // Retrieve the existing email template
            var existingEmailTemplate = _dbContext.EmailTemplates
                .FirstOrDefault(et => et.TemplateId == id && et.FkCompanyId == CompanyId);

            // Check if the email template exists
            if (existingEmailTemplate == null)
            {
                return NotFound();
            }

            // Update the existing email template properties
            existingEmailTemplate.Subject = updatedEmailTemplateDto.Subject;
            existingEmailTemplate.Stage = updatedEmailTemplateDto.Stage;
            existingEmailTemplate.EmailTemplate1 = updatedEmailTemplateDto.EmailTemplate1;
            existingEmailTemplate.Body = updatedEmailTemplateDto.Body;
            // Update other properties as needed

            // Set the LastModified property to the current date and time
            existingEmailTemplate.LastModified = DateTime.Now;

            // Save changes to the database
            _dbContext.SaveChanges();

            return Ok("Email template updated successfully");
        }

        // POST: api/EmailTemplate
        [HttpPost]
        public ActionResult<EmailTemplate> PostEmailTemplate([FromBody] EmailTemplate newEmailTemplate)
        {
            try
            {
                // Fetch the company ID from the claims or other authentication mechanism
                var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

                if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int loggedInCompanyId))
                {
                    return BadRequest("Invalid data. Company information is missing.");
                }

                // Set the FkCompanyId and LastModified fields
                newEmailTemplate.FkCompanyId = loggedInCompanyId;
                newEmailTemplate.LastModified = DateTime.Now;

                // Additional fields can be added here based on your requirements
                // newEmailTemplate.YourField = "Your Value";

                _dbContext.EmailTemplates.Add(newEmailTemplate);
                _dbContext.SaveChanges();

                return CreatedAtAction(nameof(GetEmailTemplate), new { id = newEmailTemplate.TemplateId }, newEmailTemplate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE: api/EmailTemplate/5
        [HttpDelete("{id}")]
        public IActionResult DeleteEmailTemplate(int id)
        {
            var emailTemplateToDelete = _dbContext.EmailTemplates.Find(id);

            if (emailTemplateToDelete == null)
                return NotFound();

            _dbContext.EmailTemplates.Remove(emailTemplateToDelete);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
