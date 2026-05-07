using FluentValidation;
using HouseBudget.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace HouseBudget.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, "Validation failed.",
                ve.Errors.Select(e => e.ErrorMessage).ToList()),
            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message, new List<string>()),
            UnauthorizedDomainException ude => (HttpStatusCode.Unauthorized, ude.Message, new List<string>()),
            DomainException de => (HttpStatusCode.BadRequest, de.Message, new List<string>()),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", new List<string>())
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            message,
            errors = errors.Any() ? errors : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
