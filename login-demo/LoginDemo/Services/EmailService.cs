using System.Net;
using System.Net.Mail;
using System.Text;
using LoginDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LoginDemo.Services;

public class EmailService : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        await SendEmailAsync(email, "Confirm your email", 
            $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>");
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        await SendEmailAsync(email, "Reset your password", 
            $"Your password reset code is: {resetCode}");
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        await SendEmailAsync(email, "Reset your password", 
            $"Please reset your password by clicking this link: <a href='{resetLink}'>Reset Password</a>");
    }

    private async Task SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
            if (smtpSettings == null)
            {
                _logger.LogError("SMTP settings are not configured");
                throw new InvalidOperationException("SMTP settings are not configured");
            }

            _logger.LogInformation("Attempting to send email to {Email} using SMTP server {Server}:{Port}", 
                email, smtpSettings.Server, smtpSettings.Port);

            using var client = new SmtpClient(smtpSettings.Server, smtpSettings.Port)
            {
                EnableSsl = smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings.FromEmail, smtpSettings.FromName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}. Error: {Error}", email, ex.Message);
            throw;
        }
    }
}

public class SmtpSettings
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
} 