using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace HouseBudget.IntegrationTests;

public sealed class AuthEndpointsTests(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidRequest_Returns201WithTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Ivan",
            lastName = "Test",
            email = $"test_{Guid.NewGuid():N}@example.com",
            password = "Password123!",
            currency = "USD"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Success.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrEmpty();
        body.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid():N}@example.com";
        var payload = new { firstName = "A", lastName = "B", email, password = "Password123!" };

        await _client.PostAsJsonAsync("/api/v1/auth/register", payload);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var email = $"login_{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Login", lastName = "User", email, password
        });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"wrong_{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "A", lastName = "B", email, password = "CorrectPass1!"
        });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WithValidToken_Returns200()
    {
        var email = $"profile_{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        var registerResp = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Profile", lastName = "User", email, password
        });
        var registerBody = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();
        var token = registerBody!.Data!.AccessToken;

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public record AuthResponse(bool Success, string? Message, AuthData? Data);
public record AuthData(Guid UserId, string Email, string FullName, string AccessToken, string RefreshToken);
