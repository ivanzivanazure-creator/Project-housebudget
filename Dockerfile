# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy backend solution (excludes MAUI — MAUI requires Android SDK workload)
COPY HouseBudget.Backend.sln ./

# Copy only backend csproj files first to maximise Docker layer caching.
COPY src/HouseBudget.Domain/HouseBudget.Domain.csproj                   src/HouseBudget.Domain/
COPY src/HouseBudget.Application/HouseBudget.Application.csproj         src/HouseBudget.Application/
COPY src/HouseBudget.Infrastructure/HouseBudget.Infrastructure.csproj   src/HouseBudget.Infrastructure/
COPY src/HouseBudget.API/HouseBudget.API.csproj                         src/HouseBudget.API/
COPY tests/HouseBudget.Domain.Tests/HouseBudget.Domain.Tests.csproj              tests/HouseBudget.Domain.Tests/
COPY tests/HouseBudget.Application.Tests/HouseBudget.Application.Tests.csproj    tests/HouseBudget.Application.Tests/
COPY tests/HouseBudget.IntegrationTests/HouseBudget.IntegrationTests.csproj      tests/HouseBudget.IntegrationTests/

RUN dotnet restore HouseBudget.Backend.sln

# Copy source after restore (layer-cache hit for unchanged dependencies)
COPY src/ src/
COPY tests/ tests/

# Publish API in Release mode
RUN dotnet publish src/HouseBudget.API/HouseBudget.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=build /app/publish ./

RUN mkdir -p /app/logs && chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "HouseBudget.API.dll"]
