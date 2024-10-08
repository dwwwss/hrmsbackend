using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hrms_backend.Models;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public CountryController(HrmsDbContext context)
        {
            _context = context;
        }

        // GET: api/Country
        [HttpGet]
        public IActionResult GetCountries()
        {
            var countries = _context.Countries.ToList();
            return Ok(countries);
        }
    }
}
