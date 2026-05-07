using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Interfaces;
using HouseBudget.Infrastructure.Persistence;
using HouseBudget.Infrastructure.Repositories;
using HouseBudget.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseBudget.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (connectionString?.Contains("DataSource") == true || connectionString?.Contains(".db") == true)
                options.UseSqlite(connectionString);
            else
                options.UseSqlServer(connectionString ?? "Server=(localdb)\\mssqllocaldb;Database=HouseBudget;Trusted_Connection=True;");

            options.EnableSensitiveDataLogging(false);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IPaymentGatewayService, StripePaymentService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
