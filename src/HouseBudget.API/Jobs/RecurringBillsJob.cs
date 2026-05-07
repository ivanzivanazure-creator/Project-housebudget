using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.API.Jobs;

/// <summary>
/// Runs daily: creates transactions for AutoPay bills that are due, sends reminders
/// for upcoming bills, and marks missed bills as overdue in logs.
/// </summary>
public sealed class RecurringBillsJob(
    AppDbContext db,
    IEmailService emailService,
    ILogger<RecurringBillsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await ProcessAutoPayBillsAsync(today, ct);
        await SendBillRemindersAsync(today, ct);
    }

    private async Task ProcessAutoPayBillsAsync(DateOnly today, CancellationToken ct)
    {
        var dueBills = await db.Set<Bill>()
            .Include(b => b.Account)
            .Where(b => b.IsActive && b.AutoPay && b.NextDueDate <= today && !b.IsDeleted)
            .ToListAsync(ct);

        foreach (var bill in dueBills)
        {
            try
            {
                var transaction = Transaction.CreateExpense(
                    bill.UserId, bill.AccountId, bill.CategoryId,
                    bill.Amount.Amount, bill.Amount.Currency,
                    bill.Name, today, $"Auto-pay: {bill.Name}", isRecurring: true);

                db.Set<Transaction>().Add(transaction);
                bill.Account.Debit(bill.Amount.Amount);
                bill.MarkAsPaid(today);

                logger.LogInformation("Auto-paid bill {BillName} for user {UserId}", bill.Name, bill.UserId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to auto-pay bill {BillId}", bill.Id);
            }
        }

        if (dueBills.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private async Task SendBillRemindersAsync(DateOnly today, CancellationToken ct)
    {
        var reminderBills = await db.Set<Bill>()
            .Include(b => b.Account)
            .Where(b => b.IsActive && !b.AutoPay && !b.IsDeleted
                && b.NextDueDate > today
                && b.NextDueDate <= DateOnly.FromDateTime(today.ToDateTime(TimeOnly.MinValue).AddDays(3)))
            .Join(db.Users, b => b.UserId, u => u, (b, u) => new { Bill = b, User = u })
            .ToListAsync(ct);

        foreach (var item in reminderBills)
        {
            try
            {
                var daysUntilDue = item.Bill.NextDueDate.DayNumber - today.DayNumber;
                await emailService.SendBillReminderAsync(
                    item.User.Email.Value,
                    item.User.FullName,
                    item.Bill.Name,
                    item.Bill.Amount.Amount,
                    item.Bill.Amount.Currency,
                    item.Bill.NextDueDate,
                    daysUntilDue,
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send bill reminder for bill {BillId}", item.Bill.Id);
            }
        }
    }
}
