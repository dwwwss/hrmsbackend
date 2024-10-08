using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hrms_backend.Models;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public CityController(HrmsDbContext context)
        {
            _context = context;
        }

        // GET: api/City/{stateId}
        [HttpGet("{stateId}")]
        public IActionResult GetCitiesByState(int stateId)
        {
            var cities = _context.Cities
                .Where(c => c.StateId == stateId)
                .ToList();

            return Ok(cities);
        }
    }
}
