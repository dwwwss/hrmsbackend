/*using System;
using System.Net;
using System.Net.Mail;
using hrms_backend.Models;
using static hrms_backend.Controllers.CompanyCreate;
using static hrms_backend.Controllers.NewsController;

public class EmailService : IEmailService
{
    private readonly string smtpHost = "smtp.gmail.com";
    private readonly int smtpPort = 587;
    private readonly string smtpUsername = "dpatidar1221@gmail.com";
    private readonly string smtpPassword = "lveh yhaw szhz lydf"; // replace with your Gmail email password

    public void SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                // Add recipient email
                mailMessage.To.Add(toEmail);

                client.Send(mailMessage);

                Console.WriteLine($"Email sent to: {toEmail}\nSubject: {subject}\nBody: {body}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            // Handle the exception as needed in your application
        }
    }
}


*/