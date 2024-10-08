using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hrms_backend.Models;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public StateController(HrmsDbContext context)
        {
            _context = context;
        }

        // GET: api/State/{countryId}
        [HttpGet("{countryId}")]
        public IActionResult GetStatesByCountry(int countryId)
        {
            var states = _context.Statesses
                .Where(s => s.CountryId == countryId)
                .ToList();

            return Ok(states);
        }
    }
}
