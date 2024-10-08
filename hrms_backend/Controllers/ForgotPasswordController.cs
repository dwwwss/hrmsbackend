using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using hrms_backend.Models;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static hrms_backend.Controllers.NewsController;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;
        private readonly IOptions<EmailConfiguration> _emailConfigOptions;

        public ForgotPasswordController(HrmsDbContext dbContext, IOptions<EmailConfiguration> emailConfigOptions)
        {
            _dbContext = dbContext;
            _emailConfigOptions = emailConfigOptions;
        }
        [HttpPost("SendResetLink")]
        public async Task<IActionResult> SendResetLink([FromBody] ForgotPasswordRequestModel model)
        {
            // TODO: Validate the email address and check if it exists in the database
            // For simplicity, let's assume the email is valid

            // Generate a unique token
            string resetToken = Guid.NewGuid().ToString();
            Employee employee = _dbContext.Employees.FirstOrDefault(e => e.Email == model.Email);

            if (employee != null)
            {
                // Check if ResetToken is null before using it
                if (employee.ResetToken == null)
                {
                    // Save the generated token to ResetToken only if it's currently null
                    employee.ResetToken = resetToken;
                    _dbContext.SaveChanges();
                }

                // Build the reset link
                string resetLink = $"http://10.0.0.168/#/reset?token={employee.ResetToken}";
                SendResetPasswordEmail(model.Email, resetLink);

                // TODO: Return a success response
                return Ok("Please Check Your email");
            }
            else
            {
                // Handle the case where the user with the specified email is not found
                return BadRequest("User not found with the specified email address.");
            }
        }


        private void SendResetPasswordEmail(string toEmail, string resetLink)
        {
            // Update the following details based on your SMTP server and email template
            EmailConfiguration emailConfig = _emailConfigOptions.Value;

            // Access configuration values
            string smtpServer = emailConfig.SmtpServer;
            int smtpPort = emailConfig.SmtpPort;
            string smtpUsername = emailConfig.SmtpUsername;
            string smtpPassword = emailConfig.SmtpPassword;
            bool enableSsl = emailConfig.EnableSsl;
            string fromEmail = emailConfig.FromEmail;
            string fromDisplayName = emailConfig.FromDisplayName;
            string subject = "Password Reset";

            // Improved and stylized HTML body with a reset password message
            string body = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: 'Arial', sans-serif;
                    background-color: #f4f4f4;
                    padding: 20px;
                }}
                .container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background-color: #ffffff;
                    padding: 20px;
                    border-radius: 10px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                }}
                p {{
                    color: #555555;
                    line-height: 1.6;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <p>Dear User,</p>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetLink}'>{resetLink}</a></p>
                <p>If you did not initiate this password reset, please contact our support team.</p>
                <p>Best regards,<br>HRMS Team</p>
            </div>
        </body>
        </html>
    ";

            using (MailMessage mailMessage = new MailMessage())
            {
                MailAddress fromAddress = new MailAddress(fromEmail, fromDisplayName);
                mailMessage.From = fromAddress;

                // Set the "Reply-To" address that recipients will see
                mailMessage.ReplyToList.Add(new MailAddress("supporthrms@gmail.com", fromDisplayName));

                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = enableSsl;

                    try
                    {
                        smtpClient.Send(mailMessage);
                        Console.WriteLine($"Reset password email sent to {toEmail}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending reset password email: {ex.Message}");
                        // Handle the error appropriately (e.g., log it or inform the user)
                    }
                }
            }
        }
    }
}
public class ForgotPasswordRequestModel
{
    public string Email { get; set; }
}

