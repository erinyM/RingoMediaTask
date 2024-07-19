using System.Net.Mail;
using System.Net;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ReminderService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _emailFrom = "erenyaspmails@gmail.com";
        private readonly string _emailPassword = "jnmt wjdd azns fefc"; // Use App Password if 2-Step Verification is enabled
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(IServiceScopeFactory scopeFactory, ILogger<ReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckReminders, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private void CheckReminders(object state)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var reminders = context.Reminders
                        .Where(r => r.ReminderDateTime >= DateTime.Now && !r.IsNotified)
                        .ToList();

                    foreach (var reminder in reminders)
                    {
                        // Send email logic
                        SendEmail(reminder);

                        reminder.IsNotified = true;
                        context.Reminders.Update(reminder);
                    }

                    context.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("sent mail failed " + ex);

            }
        }

        private void SendEmail(Reminder reminder)
        {
            var emailBody = GenerateEmailBody(reminder);
            // Implement email sending logic here
            //please replace eriny2015@yahoo.com with your email to test it
            var email = new MailMessage(_emailFrom, "eriny2015@yahoo.com")
            {
                Subject = "Ringo Reminder",
                //  Body = $"Reminder: {reminder.Title} at {reminder.ReminderDateTime}"
                Body = emailBody,
                IsBodyHtml = true
            };
            using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_emailFrom, _emailPassword);
                smtpClient.EnableSsl = true;
                try
                {
                    smtpClient.Send(email);
                    Console.WriteLine("Email sent successfully.");
                }
                catch (SmtpException ex)
                {
                    Console.WriteLine($"SMTP Exception: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Exception: {ex.Message}");
                }
            }
        }
        // design html format for email
        private string GenerateEmailBody(Reminder reminder)
        {
            const string htmlTemplate = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        .email-container {
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            padding: 20px;
                            border: 1px solid #ddd;
                            border-radius: 5px;
                            max-width: 600px;
                            margin: auto;
                        }
                        .email-header {
                            background-color: #f4f4f4;
                            padding: 10px;
                            border-bottom: 1px solid #ddd;
                            text-align: center;
                        }
                        .email-body {
                            padding: 20px;
                        }
                        .email-footer {
                            text-align: center;
                            padding: 10px;
                            font-size: 12px;
                            color: #777;
                        }
                        .email-footer a {
                            color: #777;
                            text-decoration: none;
                        }
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <h1>Reminder Notification</h1>
                        </div>
                        <div class='email-body'>
                            <p>Dear User,</p>
                            <p>You have a new reminder:</p>
                            <p><strong>{{Title}}</strong> at <strong>{{ReminderDateTime}}</strong></p>
                            <p>Please ensure to take necessary actions.</p>
                        </div>
                        <div class='email-footer'>
                            <p>Thank you for using our service.</p>
                            <p><a href='http://RingoMedia.com'>Your Company</a></p>
                        </div>
                    </div>
                </body>
                </html>";

                    return htmlTemplate
                        .Replace("{{Title}}", reminder.Title)
                        .Replace("{{ReminderDateTime}}", reminder.ReminderDateTime.ToString());
                }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}