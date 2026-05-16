using HouseBudget.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseBudget.IntegrationTests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Provide required configuration values so Program.cs doesn't throw.
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSecretKey_MustBe32CharsOrMoreForHS256!",
                ["JwtSettings:Issuer"]    = "HouseBudgetAPI",
                ["JwtSettings:Audience"]  = "HouseBudgetClient",
                ["ConnectionStrings:DefaultConnection"] = "InMemory"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace real DB with a unique InMemory database per test run.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase($"HouseBudget_Test_{Guid.NewGuid()}"));
        });
    }
}
