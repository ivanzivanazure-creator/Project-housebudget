# HouseBudget — Personal Finance Management

A market-ready personal finance app built with **.NET 8 Clean Architecture + DDD** and **.NET MAUI Android**.

## Architecture

```
HouseBudget/
├── src/
│   ├── HouseBudget.Domain/          # DDD Core — Aggregates, Value Objects, Domain Events
│   ├── HouseBudget.Application/     # Use Cases — CQRS (MediatR), FluentValidation, DTOs
│   ├── HouseBudget.Infrastructure/  # EF Core, JWT, Repositories, Email (MailKit)
│   ├── HouseBudget.API/             # ASP.NET Core 8 Web API — Swagger, JWT Auth
│   └── HouseBudget.Mobile/          # .NET MAUI Android App — MVVM, CommunityToolkit
└── tests/
    ├── HouseBudget.Domain.Tests/
    └── HouseBudget.Application.Tests/
```

## Design Principles

- **Clean Architecture** — strict dependency rule (Domain ← Application ← Infrastructure ← API)
- **Domain-Driven Design** — Aggregates, Value Objects, Domain Events, Repository pattern
- **SOLID** — Single Responsibility, Open/Closed, Liskov, Interface Segregation, Dependency Inversion
- **CQRS** — Commands and Queries via MediatR with pipeline behaviors
- **Pipeline Behaviors** — Validation (FluentValidation) and Logging cross-cutting concerns

## Market Features

| Feature | Description |
|---------|-------------|
| Authentication | JWT + Refresh Tokens, secure storage |
| Multiple Accounts | Checking, Savings, Credit Card, Cash, Investment, Loan |
| Income Tracking | Multiple sources, recurring income |
| Expense Tracking | Manual entry, merchant, location, receipt photos, tags |
| Budget Management | Monthly/Quarterly/Annual budgets by category, overspend alerts |
| Categories | 25+ system categories + custom user categories |
| Savings Goals | Progress tracking, monthly contribution targets, milestones |
| Bills & Recurring | Reminders, overdue alerts, auto-pay tracking |
| Financial Reports | Monthly & annual reports, category breakdowns, daily spending |
| Dashboard | Net worth, cash flow, spending trends, upcoming bills |
| Email Notifications | Welcome email, budget alerts, bill reminders |
| Multi-currency | Full currency support via Money value object |
| Soft Delete | All entities support soft delete |

## Domain Layer

**Aggregates:**
- `User` — authentication, profile, preferences
- `Account` — bank accounts with real-time balance tracking
- `Transaction` — income, expense, transfer with full audit trail
- `Budget` — period budgets with per-category allocation and spend tracking
- `Goal` — savings goals with contributions and milestone events
- `Bill` — recurring bills with next-due-date calculations

**Value Objects:** `Money`, `Email`, `DateRange`

**Domain Events:** `UserRegistered`, `TransactionCreated`, `BudgetCreated`, `BudgetExceeded`, `GoalAchieved`, `BillPaid`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/v1/auth/register | Register new user |
| POST | /api/v1/auth/login | Login |
| POST | /api/v1/auth/refresh-token | Refresh JWT |
| GET | /api/v1/dashboard | Full dashboard data |
| CRUD | /api/v1/accounts | Bank accounts |
| CRUD | /api/v1/transactions/expenses | Expenses |
| CRUD | /api/v1/transactions/income | Income |
| CRUD | /api/v1/budgets | Budgets |
| CRUD | /api/v1/goals | Savings goals |
| GET | /api/v1/goals/{id}/contribute | Contribute to goal |
| CRUD | /api/v1/bills | Recurring bills |
| GET | /api/v1/bills/upcoming | Upcoming bills |
| GET | /api/v1/bills/overdue | Overdue bills |
| GET | /api/v1/categories | Categories |
| GET | /api/v1/reports/monthly/{year}/{month} | Monthly report |
| GET | /api/v1/reports/annual/{year} | Annual report |

## Getting Started

### Backend API

```bash
cd src/HouseBudget.API
dotnet run
# Swagger UI: http://localhost:5000
```

### Android App

```bash
cd src/HouseBudget.Mobile
dotnet build -t:Run -f net8.0-android
```

### Run Tests

```bash
dotnet test
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| API | ASP.NET Core 8, Minimal APIs ready |
| ORM | Entity Framework Core 8 (SQLite / SQL Server) |
| CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| Auth | JWT Bearer + BCrypt.Net |
| Email | MailKit |
| Logging | Serilog |
| Android | .NET MAUI 8, CommunityToolkit.Mvvm |
| Tests | xUnit, FluentAssertions, Moq |
| Docs | Swagger / OpenAPI |
