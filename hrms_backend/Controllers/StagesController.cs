using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using hrms_backend.Models;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all actions in this controller
    public class StagesController : ControllerBase
    {
        private readonly HrmsDbContext dbContext;

        public StagesController(HrmsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // GET: api/Stages
        [HttpGet]
        public ActionResult<IEnumerable<Stage>> Get()
        {
            // Get the CompanyId from the user's claims
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim == null)
                return Forbid(); // User is not authenticated or doesn't have a CompanyId claim

            int companyId = int.Parse(companyIdClaim.Value);

            // Retrieve stages only for the authenticated user's company
            var stages = dbContext.Stages.Where(s => s.FkCompanyId == companyId).ToList();
            return Ok(stages);
        }

        // GET: api/Stages/5
        [HttpGet("{id}")]
        public ActionResult<Stage> GetById(int id)
        {
            var stage = dbContext.Stages.Find(id);

            if (stage == null)
                return NotFound();

            // Check if the user has access to the requested stage's company
            if (!User.HasClaim("CompanyId", stage.FkCompanyId.ToString()))
                return Forbid(); // User doesn't have access to the requested company

            return Ok(stage);
        }

        // POST: api/Stages
        // POST: api/Stages
        [HttpPost]
        public ActionResult<Stage> Post([FromBody] Stage newStage)
        {
            // Check if the user has the "CompanyId" claim
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim == null)
                return Forbid(); // User doesn't have the "CompanyId" claim

            // Set the FkCompanyId property of the new stage to the user's CompanyId
            if (!int.TryParse(companyIdClaim.Value, out int companyId))
                return BadRequest("Invalid CompanyId claim");

            newStage.FkCompanyId = companyId;

            // Add the new stage to the database
            dbContext.Stages.Add(newStage);
            dbContext.SaveChanges();

            // Return the newly created stage
            return CreatedAtAction(nameof(GetById), new { id = newStage.StageId }, newStage);
        }


        // PUT: api/Stages/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Stage updatedStage)
        {
            var existingStage = dbContext.Stages.Find(id);

            if (existingStage == null)
                return NotFound();

            // Check if the user has access to the specified company
            if (!User.HasClaim("CompanyId", existingStage.FkCompanyId.ToString()))
                return Forbid(); // User doesn't have access to the specified company

            existingStage.StageName = updatedStage.StageName;

            dbContext.SaveChanges();

            return NoContent();
        }

        // DELETE: api/Stages/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var stageToDelete = dbContext.Stages.Find(id);

            if (stageToDelete == null)
                return NotFound();

            // Check if the user has access to the specified company
            if (!User.HasClaim("CompanyId", stageToDelete.FkCompanyId.ToString()))
                return Forbid(); // User doesn't have access to the specified company

            dbContext.Stages.Remove(stageToDelete);
            dbContext.SaveChanges();

            return NoContent();
        }
    }
}
