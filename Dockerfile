# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy solution file and all csproj files first to maximise Docker layer caching.
# dotnet restore will only re-run when these files change.
COPY HouseBudget.sln ./

COPY src/HouseBudget.API/HouseBudget.API.csproj                         src/HouseBudget.API/
COPY src/HouseBudget.Application/HouseBudget.Application.csproj         src/HouseBudget.Application/
COPY src/HouseBudget.Domain/HouseBudget.Domain.csproj                   src/HouseBudget.Domain/
COPY src/HouseBudget.Infrastructure/HouseBudget.Infrastructure.csproj   src/HouseBudget.Infrastructure/
COPY src/HouseBudget.Mobile/HouseBudget.Mobile.csproj                   src/HouseBudget.Mobile/
COPY tests/HouseBudget.Application.Tests/HouseBudget.Application.Tests.csproj  tests/HouseBudget.Application.Tests/
COPY tests/HouseBudget.Domain.Tests/HouseBudget.Domain.Tests.csproj             tests/HouseBudget.Domain.Tests/

# Restore dependencies for the entire solution.
# The Mobile project targets Android and will not restore on Linux; exclude it.
RUN dotnet restore HouseBudget.sln \
    --ignore-failed-sources \
    /p:ExcludeProjects="src/HouseBudget.Mobile/HouseBudget.Mobile.csproj"

# Copy the remaining source (everything under src/ — MAUI excluded from publish).
COPY src/ src/
COPY tests/ tests/

# Publish the API in Release mode.
RUN dotnet publish src/HouseBudget.API/HouseBudget.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for security.
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

# Copy published output from the build stage.
COPY --from=build /app/publish ./

# Ensure the logs directory exists and is writable by the app user.
RUN mkdir -p /app/logs && chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080

# Bind to port 8080; Kestrel will not use HTTPS inside the container —
# TLS termination is handled by the reverse proxy / load balancer.
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "HouseBudget.API.dll"]
