// ValidationController
using hrms_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Validation Controller
    public class ValidationController : Controller
    {
        private readonly HrmsDbContext _dbContext;

        public ValidationController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public JsonResult IsEmailAvailable(string email)
        {
            bool isAvailable = !_dbContext.Employees.Any(e => e.Email == email);
            return Json(isAvailable);
        }
    }
}
