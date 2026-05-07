using HouseBudget.Application.Commands.Payments;
using HouseBudget.Application.Interfaces;
using HouseBudget.Application.Queries.Payments;
using HouseBudget.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HouseBudget.API.Controllers;

/// <summary>Payment processing, history, and refunds</summary>
[Authorize]
public sealed class PaymentsController : BaseApiController
{
    private readonly IPaymentGatewayService _gateway;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public PaymentsController(IPaymentGatewayService gateway, IPaymentRepository paymentRepo, ISubscriptionRepository subscriptionRepo, ISubscriptionPlanRepository planRepo, ICurrentUserService currentUser, IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _gateway = gateway;
        _paymentRepo = paymentRepo;
        _subscriptionRepo = subscriptionRepo;
        _planRepo = planRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    /// <summary>Get payment history (paginated)</summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Success(await Mediator.Send(new GetPaymentHistoryQuery(page, pageSize), ct));

    /// <summary>Get a specific payment by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var payment = await _paymentRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Payment", id);
        if (payment.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        var plan = await _planRepo.GetByIdAsync(payment.PlanId, ct);
        return Success(HouseBudget.Application.Commands.Payments.PaymentMapper.ToDto(payment, plan?.Name ?? "Unknown"));
    }

    /// <summary>Create a Stripe payment intent for a one-time charge</summary>
    [HttpPost("intent")]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));

    /// <summary>Request a refund for a payment</summary>
    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundRequest request, CancellationToken ct)
        => Success(await Mediator.Send(new RefundPaymentCommand(id, request.Amount, request.Reason), ct));

    /// <summary>Stripe webhook endpoint — processes subscription/payment events</summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var payload = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = _configuration["Stripe:WebhookSecret"] ?? string.Empty;

        var webhookEvent = _gateway.ParseWebhookEvent(payload, signature, webhookSecret);
        if (webhookEvent is null) return BadRequest("Invalid webhook signature.");

        await HandleWebhookEventAsync(webhookEvent, ct);
        return Ok();
    }

    private async Task HandleWebhookEventAsync(GatewayWebhookEvent webhookEvent, CancellationToken ct)
    {
        switch (webhookEvent.EventType)
        {
            case "invoice.payment_succeeded":
            {
                if (webhookEvent.SubscriptionId is null) break;
                var sub = await _subscriptionRepo.GetByExternalIdAsync(webhookEvent.SubscriptionId, ct);
                if (sub is null) break;

                var plan = await _planRepo.GetByIdAsync(sub.PlanId, ct);
                if (plan is null) break;

                var payment = Domain.Entities.Payment.Create(
                    sub.UserId, sub.Id, sub.PlanId,
                    webhookEvent.Amount ?? plan.Price.Amount,
                    webhookEvent.Currency ?? plan.Price.Currency,
                    Domain.Enums.PaymentMethod.CreditCard,
                    sub.BillingPeriod,
                    sub.CurrentPeriodStart, sub.CurrentPeriodEnd,
                    webhookEvent.PaymentIntentId);

                payment.MarkCompleted();
                sub.Renew();

                await _paymentRepo.AddAsync(payment, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                break;
            }
            case "invoice.payment_failed":
            {
                if (webhookEvent.SubscriptionId is null) break;
                var sub = await _subscriptionRepo.GetByExternalIdAsync(webhookEvent.SubscriptionId, ct);
                if (sub is null) break;
                sub.MarkPastDue();
                await _unitOfWork.SaveChangesAsync(ct);
                break;
            }
            case "customer.subscription.deleted":
            {
                if (webhookEvent.SubscriptionId is null) break;
                var sub = await _subscriptionRepo.GetByExternalIdAsync(webhookEvent.SubscriptionId, ct);
                if (sub?.IsActive == true)
                {
                    sub.Expire();
                    await _unitOfWork.SaveChangesAsync(ct);
                }
                break;
            }
        }
    }
}

public record RefundRequest(decimal Amount, string Reason);
