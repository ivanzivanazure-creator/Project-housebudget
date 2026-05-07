using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.Interfaces;

public interface IPaymentGatewayService
{
    /// <summary>Creates a customer in the payment gateway and returns external customer ID.</summary>
    Task<string> CreateCustomerAsync(string email, string name, CancellationToken cancellationToken = default);

    /// <summary>Creates a subscription and returns external subscription ID.</summary>
    Task<GatewaySubscriptionResult> CreateSubscriptionAsync(string customerId, string priceId, bool withTrial, int trialDays, CancellationToken cancellationToken = default);

    /// <summary>Cancels a subscription in the payment gateway.</summary>
    Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, bool atPeriodEnd, CancellationToken cancellationToken = default);

    /// <summary>Upgrades or downgrades a subscription plan.</summary>
    Task<bool> ChangeSubscriptionPlanAsync(string externalSubscriptionId, string newPriceId, CancellationToken cancellationToken = default);

    /// <summary>Creates a payment intent and returns client secret for frontend.</summary>
    Task<GatewayPaymentResult> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, string description, CancellationToken cancellationToken = default);

    /// <summary>Issues a refund.</summary>
    Task<string?> RefundPaymentAsync(string externalPaymentId, decimal amount, string reason, CancellationToken cancellationToken = default);

    /// <summary>Validates a webhook signature.</summary>
    bool ValidateWebhookSignature(string payload, string signature, string secret);

    /// <summary>Parses a webhook event.</summary>
    GatewayWebhookEvent? ParseWebhookEvent(string payload, string signature, string secret);
}

public record GatewaySubscriptionResult(
    string SubscriptionId,
    string CustomerId,
    string? ClientSecret,
    DateTime CurrentPeriodEnd,
    bool IsTrialing
);

public record GatewayPaymentResult(
    string PaymentIntentId,
    string ClientSecret,
    string Status
);

public record GatewayWebhookEvent(
    string EventType,
    string EventId,
    string? SubscriptionId,
    string? PaymentIntentId,
    string? CustomerId,
    decimal? Amount,
    string? Currency,
    string? FailureReason,
    Dictionary<string, string> Metadata
);
