using Hangfire;
using HouseBudget.API.Jobs;
using HouseBudget.API.Middleware;
using HouseBudget.API.Services;
using HouseBudget.Application;
using HouseBudget.Application.Interfaces;
using HouseBudget.Infrastructure.DependencyInjection;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/housebudget-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Application + Infrastructure ──────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Current user ──────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ── JWT Authentication ─────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "HouseBudgetAPI",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "HouseBudgetClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

// ── Rate Limiting (ASP.NET Core built-in) ─────────────────────────────────
var rlConfig = builder.Configuration.GetSection("RateLimiting");

builder.Services.AddRateLimiter(options =>
{
    // Strict window for login/register
    options.AddFixedWindowLimiter("auth", cfg =>
    {
        cfg.Window = TimeSpan.FromSeconds(rlConfig.GetValue("LoginWindowSeconds", 60));
        cfg.PermitLimit = rlConfig.GetValue("LoginMaxAttempts", 10);
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 0;
    });

    // Sliding window for all other API traffic
    options.AddSlidingWindowLimiter("global", cfg =>
    {
        cfg.Window = TimeSpan.FromSeconds(rlConfig.GetValue("GlobalWindowSeconds", 60));
        cfg.PermitLimit = rlConfig.GetValue("GlobalMaxRequests", 300);
        cfg.SegmentsPerWindow = 6;
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.Headers["Retry-After"] = "60";
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { success = false, message = "Too many requests. Please slow down." }, token);
    };
});

// ── CORS ───────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        if (builder.Environment.IsDevelopment() || allowedOrigins.Length == 0)
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

// ── Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// ── Hangfire ───────────────────────────────────────────────────────────────
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2;
    options.Queues = ["default", "critical"];
});

builder.Services.AddScoped<RecurringBillsJob>();

// ── Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ── Swagger ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HouseBudget API",
        Version = "v1",
        Description = "Personal finance and house budget management API",
        Contact = new OpenApiContact { Name = "HouseBudget Team", Email = "support@housebudget.com" }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// ── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Database: run migrations then seed ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // InMemory provider (used in integration tests) doesn't support migrations.
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();

    await DatabaseSeeder.SeedAsync(db);
}

// ── Middleware pipeline ────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HouseBudget API v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();
app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();

// ── Health check endpoints ─────────────────────────────────────────────────
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// ── Controllers with global rate limit ────────────────────────────────────
app.MapControllers().RequireRateLimiting("global");

// ── Recurring Hangfire jobs ───────────────────────────────────────────────
RecurringJob.AddOrUpdate<RecurringBillsJob>(
    "process-recurring-bills",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily(6)); // daily at 06:00 UTC

app.Run();

// Expose Program class for WebApplicationFactory in integration tests
public partial class Program { }
