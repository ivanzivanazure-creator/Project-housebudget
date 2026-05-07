using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace HouseBudget.IntegrationTests;

public sealed class SubscriptionEndpointsTests(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetPlans_Anonymous_Returns200WithPlans()
    {
        var response = await _client.GetAsync("/api/v1/subscriptions/plans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PlansResponse>();
        body!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetMySubscription_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/subscriptions/my");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Subscribe_FreePlan_Returns201()
    {
        await AuthenticateAsync();

        // Get available plans
        var plansResp = await _client.GetAsync("/api/v1/subscriptions/plans");
        var plans = await plansResp.Content.ReadFromJsonAsync<PlansResponse>();
        var freePlan = plans!.Data!.First(p => p.Tier == "Free");

        var response = await _client.PostAsJsonAsync("/api/v1/subscriptions", new { planId = freePlan.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private async Task AuthenticateAsync()
    {
        var email = $"sub_{Guid.NewGuid():N}@example.com";
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Sub", lastName = "User", email, password = "Password123!"
        });
        var body = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Data!.AccessToken);
    }
}

public record PlansResponse(bool Success, string? Message, List<PlanData>? Data);
public record PlanData(Guid Id, string Name, string Tier, decimal Price);
