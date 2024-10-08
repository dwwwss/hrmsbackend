using hrms_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResetPasswordController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;
        private readonly IOptions<EmailConfiguration> _emailConfigOptions;

        public ResetPasswordController(HrmsDbContext dbContext, IOptions<EmailConfiguration> emailConfigOptions)
        {
            _dbContext = dbContext;
            _emailConfigOptions = emailConfigOptions;
        }
        [HttpPost("ResetPassword", Name = "ResetPassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            // Retrieve the user based on the token
            Employee employee = _dbContext.Employees.FirstOrDefault(e => e.ResetToken == model.Token);

            if (employee != null)
            {
                // Validate the new password
                if (!IsValidPassword(model.NewPassword))
                {
                    // Password does not meet the requirements
                    return BadRequest("Invalid password. Password must have at least 8 characters, including uppercase and lowercase letters, numbers, and one special character.");
                }

                // Set the new password without hashing (Note: You should hash the password in a real-world scenario)
                employee.Password = model.NewPassword;

                // Reset the ResetToken to null after using it
                employee.ResetToken = null;

                // Save changes to the database
                _dbContext.SaveChanges();

                // Send congratulatory email
                SendCongratulatoryEmail(employee.Email);

                // Redirect the user to a success page or provide a success message
                return Ok("Password reset successfully!");
            }
            else
            {
                // Handle the case where the user with the specified token is not found
                return BadRequest("Invalid or expired token.");
            }
        }

        private bool IsValidPassword(string password)
        {
            // Password must have at least 8 characters, including uppercase and lowercase letters, numbers, and one special character
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$%^&+=]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }
        private void SendCongratulatoryEmail(string toEmail)
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
            string subject = "Congratulations! Your Password has been Reset";

            // Improved and stylized HTML body with a congratulatory message
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
                <p>Congratulations! Your password has been successfully reset.</p>
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
                        Console.WriteLine($"Congratulatory email sent to {toEmail}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending congratulatory email: {ex.Message}");
                        // Handle the error appropriately (e.g., log it or inform the user)
                    }
                }
            }
        }
    }
}

public class ResetPasswordModel
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
