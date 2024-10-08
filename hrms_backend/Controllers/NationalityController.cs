using System.Linq;
using hrms_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NationalityController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public NationalityController(HrmsDbContext context)
        {
            _context = context;
        }

        // GET: api/Nationality
        [HttpGet]
        public IActionResult GetNationalities()
        {
            try
            {
                var nationalities = _context.Nationalities.ToList();

                if (nationalities == null || !nationalities.Any())
                {
                    return NotFound("No nationalities found");
                }

                return Ok(nationalities);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
