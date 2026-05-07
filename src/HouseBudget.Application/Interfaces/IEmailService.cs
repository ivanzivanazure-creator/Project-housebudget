namespace HouseBudget.Application.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default);
    Task SendBillReminderEmailAsync(string toEmail, string billName, decimal amount, DateOnly dueDate, CancellationToken cancellationToken = default);
    Task SendBudgetAlertEmailAsync(string toEmail, string budgetName, decimal spent, decimal budget, CancellationToken cancellationToken = default);
}
