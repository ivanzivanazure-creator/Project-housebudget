using HouseBudget.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace HouseBudget.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
        => await SendEmailAsync(toEmail, "Welcome to HouseBudget!", $"Hi {userName},\n\nWelcome to HouseBudget! Your account has been created.\n\nStart tracking your finances today.", cancellationToken);

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
        => await SendEmailAsync(toEmail, "Reset Your Password", $"Use this token to reset your password: {resetToken}\n\nThis token expires in 1 hour.", cancellationToken);

    public async Task SendBillReminderEmailAsync(string toEmail, string billName, decimal amount, DateOnly dueDate, CancellationToken cancellationToken = default)
        => await SendEmailAsync(toEmail, $"Bill Reminder: {billName}", $"Your bill '{billName}' of {amount:C} is due on {dueDate:MMMM dd, yyyy}.", cancellationToken);

    public async Task SendBudgetAlertEmailAsync(string toEmail, string budgetName, decimal spent, decimal budget, CancellationToken cancellationToken = default)
        => await SendEmailAsync(toEmail, $"Budget Alert: {budgetName}", $"You have spent {spent:C} out of your {budget:C} budget for '{budgetName}'.", cancellationToken);

    private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings["FromName"] ?? "HouseBudget", emailSettings["FromEmail"] ?? "noreply@housebudget.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(emailSettings["SmtpHost"], int.Parse(emailSettings["SmtpPort"] ?? "587"), false, cancellationToken);
            if (!string.IsNullOrEmpty(emailSettings["SmtpUser"]))
                await client.AuthenticateAsync(emailSettings["SmtpUser"], emailSettings["SmtpPassword"], cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}
