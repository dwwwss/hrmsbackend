  using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hrms_backend.Models; // Assuming you have a News model
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly HrmsDbContext _context; // Replace YourDbContext with your actual database context
        public NewsController(HrmsDbContext context)
        {
            _context = context;
        }
        // GET: api/News
        [HttpGet]
        public async Task<IActionResult> GetNews()
        {
            // Retrieve company ID from the claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                // Handle the case where company information is not available in the token
                return BadRequest("Invalid data. Company information is missing.");
            }
            // Retrieve news with employee names for the specific company
            var news = await _context.News
                .Where(n => n.FkCompanyId == companyId)
                .Join(
                    _context.Employees,
                    news => news.FkEmployeeId,
                    employee => employee.EmployeeId,
                    (news, employee) => new
                    {
                        news.NewsId,
                        news.Title,
                        news.Content,
                        news.PublishedDate,
                        // Add other news properties as needed
                        EmployeeName = employee.FullName
                    })
                .ToListAsync();
            return Ok(news);
        }
        [HttpPost]
        public async Task<IActionResult> PostNews([FromBody] News news)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            news.PublishedDate = DateTime.Now;

            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId) ||
                employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int employeeId))
            {
                return BadRequest("Invalid data. Company information or sender's email is missing.");
            }

            // Get the sender's email using the employeeId
            var senderEmail = await _context.Employees
                .Where(e => e.FkCompanyId == companyId && e.EmployeeId == employeeId)
                .Select(e => e.Email)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                return BadRequest("Invalid data. Sender's email not found.");
            }

            news.FkCompanyId = companyId;
            news.FkEmployeeId = employeeId;

            var allEmployeeEmails = await _context.Employees
                .Where(e => e.FkCompanyId == companyId)
                .Select(e => e.Email)
                .ToListAsync();

            await EmailService.SendNewsToEmails(news, allEmployeeEmails, senderEmail, senderEmail);

            _context.News.Add(news);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetNews", new { id = news.NewsId }, news);
        }

        public class EmailService
        {
            public static async Task SendNewsToEmails(News news, List<string> recipientEmails, string senderEmail, string senderDisplayName)
            {
                if (recipientEmails == null || !recipientEmails.Any())
                {
                    Console.WriteLine("Recipient email list is null or empty.");
                    return;
                }

                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string smtpUsername = "dpatidar1221@gmail.com";
                string smtpPassword = "lveh yhaw szhz lydf";

                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(senderEmail, senderDisplayName);
                    mailMessage.Subject = news.Title;

                    // Create an HTML body for the email
                    string emailBody = $@"
                <html>
                    <body>
                        <h2>{news.Title}</h2>
                        <p><strong>Published Date:</strong> {news.PublishedDate}</p>
                        <p><strong>Content:</strong> {news.Content}</p>
                        <p><em>Sent by: {senderDisplayName} ({senderEmail})</em></p>
                    </body>
                </html>
            ";

                    mailMessage.Body = emailBody;
                    mailMessage.IsBodyHtml = true;

                    // Add valid recipients
                    foreach (var recipientEmail in recipientEmails.Where(IsValidEmail))
                    {
                        mailMessage.To.Add(recipientEmail);
                    }

                    try
                    {
                        // Send the email
                        await smtpClient.SendMailAsync(mailMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email: {ex.Message}");
                    }
                }
            }

            // Simple email validation method
            private static bool IsValidEmail(string email)
            {
                try
                {
                    var addr = new MailAddress(email);
                    return addr.Address == email;
                }
                catch
                {
                    return false;
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNews(int id, [FromBody] News updatedNews)
        {
            if (id != updatedNews.NewsId)
            {
                return BadRequest("Invalid NewsId in the request body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            var senderEmailClaim = User.Claims.FirstOrDefault(c => c.Type == " email");
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

            if (companyIdClaim == null || !int.TryParse(companyIdClaim.Value, out int companyId) ||
                senderEmailClaim == null || !int.TryParse(employeeIdClaim.Value, out int employeeId))
            {
                return BadRequest("Invalid data. Company information or sender's email is missing.");
            }

            var existingNews = await _context.News.FindAsync(id);

            if (existingNews == null)
            {
                return NotFound("News article not found.");
            }

            // Update properties of the existing news article
            existingNews.Title = updatedNews.Title;
            existingNews.Content = updatedNews.Content;
            existingNews.PublishedDate = updatedNews.PublishedDate;

            // Add any other properties you want to update

            existingNews.FkCompanyId = companyId;
            existingNews.FkEmployeeId = employeeId;

            _context.Entry(existingNews).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsExists(id))
                {
                    return NotFound("News article not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/News/{id}
        [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNews(int id)
    {
        var news = await _context.News.FindAsync(id);
        if (news == null)
        {
            return NotFound("News article not found.");
        }

        _context.News.Remove(news);
        await _context.SaveChangesAsync();

        return Ok("News article deleted successfully.");
    }

    private bool NewsExists(int id)
    {
        return _context.News.Any(e => e.NewsId == id);
    }
}
}
