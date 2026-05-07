using CsvHelper;
using CsvHelper.Configuration;
using HouseBudget.Application.Interfaces;
using MediatR;
using System.Globalization;

namespace HouseBudget.Application.Commands.Transactions;

public record ExportTransactionsCommand(
    DateOnly? From,
    DateOnly? To,
    string Format = "csv"
) : IRequest<ExportResult>;

public record ExportResult(byte[] Data, string ContentType, string FileName);

public sealed class ExportTransactionsCsvMap : ClassMap<TransactionExportRow>
{
    public ExportTransactionsCsvMap()
    {
        Map(m => m.Date).Name("Date");
        Map(m => m.Type).Name("Type");
        Map(m => m.Amount).Name("Amount");
        Map(m => m.Currency).Name("Currency");
        Map(m => m.Description).Name("Description");
        Map(m => m.Category).Name("Category");
        Map(m => m.Account).Name("Account");
        Map(m => m.Merchant).Name("Merchant");
        Map(m => m.Tags).Name("Tags");
        Map(m => m.Notes).Name("Notes");
    }
}

public record TransactionExportRow(
    DateOnly Date, string Type, decimal Amount, string Currency,
    string Description, string Category, string Account,
    string? Merchant, string? Tags, string? Notes);

public sealed class ExportTransactionsCommandHandler : IRequestHandler<ExportTransactionsCommand, ExportResult>
{
    private readonly ITransactionRepository _transactions;
    private readonly ICurrentUserService _currentUser;

    public ExportTransactionsCommandHandler(ITransactionRepository transactions, ICurrentUserService currentUser)
    {
        _transactions = transactions;
        _currentUser = currentUser;
    }

    public async Task<ExportResult> Handle(ExportTransactionsCommand request, CancellationToken cancellationToken)
    {
        var from = request.From ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var to = request.To ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var all = await _transactions.GetByUserIdAsync(_currentUser.UserId, 1, int.MaxValue, cancellationToken);

        var rows = all.Items
            .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new TransactionExportRow(
                t.TransactionDate, t.TypeName, t.Amount, t.Currency,
                t.Description, t.CategoryName, t.AccountName,
                t.Merchant, t.Tags.Length > 0 ? string.Join(";", t.Tags) : null, t.Notes))
            .ToList();

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
        csv.Context.RegisterClassMap<ExportTransactionsCsvMap>();
        await csv.WriteRecordsAsync(rows, cancellationToken);
        await writer.FlushAsync(cancellationToken);

        var fileName = $"transactions_{from:yyyyMMdd}_{to:yyyyMMdd}.csv";
        return new ExportResult(ms.ToArray(), "text/csv", fileName);
    }
}
