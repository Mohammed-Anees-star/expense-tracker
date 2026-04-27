# Expense Tracker

A production-quality, full-stack expense management system built with **.NET 8 Clean Architecture**, deployed to **Azure** via **Bicep IaC**, with a **Power Apps** Canvas App front-end.

---

## Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                                      │
│                                                                          │
│   ┌──────────────────────┐          ┌────────────────────────────────┐   │
│   │   Power Apps         │          │   Swagger UI / Postman         │   │
│   │   Canvas App         │          │   (dev/test)                   │   │
│   └──────────┬───────────┘          └───────────────┬────────────────┘   │
│              │ HTTPS                                │ HTTPS              │
└──────────────┼─────────────────────────────────────┼────────────────────┘
               │                                     │
┌──────────────▼─────────────────────────────────────▼────────────────────┐
│                     AZURE APP SERVICE (B1 Windows)                       │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                    ExpenseTracker.API                            │    │
│  │                                                                  │    │
│  │  Program.cs  ─►  Serilog  ─►  ExceptionHandlingMiddleware       │    │
│  │                                                                  │    │
│  │  ┌──────────────────────────────────────────────────────────┐   │    │
│  │  │                 API Layer                                │   │    │
│  │  │  GET /expenses          POST /expenses                   │   │    │
│  │  │  GET /expenses/{id}     PUT /expenses/{id}               │   │    │
│  │  │  DELETE /expenses/{id}  GET /expenses/summary            │   │    │
│  │  │  GET /health            GET /swagger                     │   │    │
│  │  └────────────────────┬─────────────────────────────────────┘   │    │
│  │                       │                                          │    │
│  │  ┌────────────────────▼─────────────────────────────────────┐   │    │
│  │  │               Application Layer                          │   │    │
│  │  │  IExpenseService  ─►  ExpenseService                     │   │    │
│  │  │  DTOs (records)   ─►  AutoMapper profiles                │   │    │
│  │  │  FluentValidation ─►  CreateExpenseRequestValidator      │   │    │
│  │  │                       UpdateExpenseRequestValidator      │   │    │
│  │  └────────────────────┬─────────────────────────────────────┘   │    │
│  │                       │                                          │    │
│  │  ┌────────────────────▼─────────────────────────────────────┐   │    │
│  │  │               Domain Layer                               │   │    │
│  │  │  Expense entity  │  Category enum                        │   │    │
│  │  │  IExpenseRepository  │  IUnitOfWork                      │   │    │
│  │  └────────────────────┬─────────────────────────────────────┘   │    │
│  │                       │                                          │    │
│  │  ┌────────────────────▼─────────────────────────────────────┐   │    │
│  │  │              Infrastructure Layer                        │   │    │
│  │  │  AppDbContext (EF Core)  ─►  ExpenseRepository           │   │    │
│  │  │  UnitOfWork  ─►  ExpenseConfiguration (Fluent API)       │   │    │
│  │  │  SqlServer provider  OR  Sqlite (local dev)              │   │    │
│  │  └────────────────────┬─────────────────────────────────────┘   │    │
│  └───────────────────────┼──────────────────────────────────────────┘   │
│                          │                                               │
└──────────────────────────┼───────────────────────────────────────────────┘
                           │
       ┌───────────────────┼────────────────────────┐
       │                   │                        │
┌──────▼──────┐   ┌────────▼────────┐   ┌──────────▼──────────┐
│  Azure SQL  │   │  Azure Key      │   │  App Settings       │
│  Database   │   │  Vault          │   │  (env vars)         │
│  (serverless│   │  ConnectionStr  │   │                     │
│   GP Gen5)  │   │  secret         │   │                     │
└─────────────┘   └─────────────────┘   └─────────────────────┘

CI/CD: GitHub Actions  ──►  build  ──►  test  ──►  deploy (main branch only)
IaC:   Azure Bicep     ──►  infra/main.bicep  ──►  infra/deploy.sh
```

---

## Project Structure

```
expense-tracker/
├── ExpenseTracker.sln
│
├── src/
│   ├── ExpenseTracker.Domain/              # No dependencies — pure domain model
│   │   ├── Entities/
│   │   │   └── Expense.cs
│   │   ├── Enums/
│   │   │   └── Category.cs
│   │   └── Interfaces/
│   │       ├── IExpenseRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   ├── ExpenseTracker.Application/         # Depends on Domain only
│   │   ├── DTOs/
│   │   │   ├── CreateExpenseRequest.cs
│   │   │   ├── UpdateExpenseRequest.cs
│   │   │   ├── ExpenseDto.cs
│   │   │   └── ExpenseSummaryDto.cs
│   │   ├── Exceptions/
│   │   │   ├── NotFoundException.cs
│   │   │   └── ValidationException.cs
│   │   ├── Interfaces/
│   │   │   └── IExpenseService.cs
│   │   ├── Mappings/
│   │   │   └── ExpenseMappingProfile.cs
│   │   ├── Services/
│   │   │   └── ExpenseService.cs
│   │   └── Validators/
│   │       ├── CreateExpenseRequestValidator.cs
│   │       └── UpdateExpenseRequestValidator.cs
│   │
│   ├── ExpenseTracker.Infrastructure/      # Depends on Domain + Application
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   └── ExpenseConfiguration.cs
│   │   │   └── Migrations/
│   │   │       └── README.md
│   │   ├── Repositories/
│   │   │   ├── ExpenseRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   └── DependencyInjection.cs
│   │
│   └── ExpenseTracker.API/                 # Depends on Application + Infrastructure
│       ├── Controllers/
│       │   └── ExpensesController.cs
│       ├── Extensions/
│       │   ├── ApplicationServiceExtensions.cs
│       │   └── SwaggerExtensions.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── appsettings.json                # Azure SQL via Key Vault reference
│       ├── appsettings.Development.json    # SQLite for local dev
│       └── Program.cs
│
├── infra/                                  # Azure IaC
│   ├── main.bicep                          # App Service, SQL, Key Vault
│   ├── main.bicepparam                     # Parameter values
│   └── deploy.sh                          # End-to-end deploy script
│
├── github-actions/
│   └── .github/
│       └── workflows/
│           └── deploy.yml                  # Build → Test → Deploy pipeline
│
└── powerapps/
    └── POWERAPPS_SETUP.md                  # Full Power Apps setup guide
```

---

## Quick Start — Local Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Optional) [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install -g dotnet-ef`

### 1. Clone and restore

```bash
git clone https://github.com/<your-org>/expense-tracker.git
cd expense-tracker
dotnet restore
```

### 2. Run locally (SQLite — no Azure needed)

The `appsettings.Development.json` already points to SQLite:

```bash
dotnet run --project src/ExpenseTracker.API
```

The API starts on `https://localhost:5001` (or `http://localhost:5000`).

- **Swagger UI**: `https://localhost:5001/swagger`
- **Health check**: `https://localhost:5001/health`

EF Core migrations run automatically on startup.

### 3. Try it out

```bash
# Create an expense
curl -X POST https://localhost:5001/expenses \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Coffee",
    "amount": 4.50,
    "category": 1,
    "date": "2026-04-27T09:00:00Z",
    "userId": "alice@example.com",
    "notes": "Morning flat white"
  }'

# List all expenses
curl "https://localhost:5001/expenses?userId=alice@example.com"

# Get spending summary
curl "https://localhost:5001/expenses/summary?userId=alice@example.com"
```

### 4. Generate / apply EF migrations (if you change entities)

```bash
# Generate
dotnet ef migrations add <MigrationName> \
  --project src/ExpenseTracker.Infrastructure \
  --startup-project src/ExpenseTracker.API \
  --output-dir Data/Migrations

# Apply
dotnet ef database update \
  --project src/ExpenseTracker.Infrastructure \
  --startup-project src/ExpenseTracker.API
```

---

## Azure Deployment

### Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) ≥ 2.50
- [Bicep CLI](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/install) (bundled with recent Azure CLI)
- `jq` installed (`brew install jq` / `apt install jq`)
- An active Azure subscription

### Step 1 — Configure parameters

Edit `infra/main.bicepparam`:

```
param baseName = 'myapp'        # 3-12 lowercase alphanumeric
param location = 'eastus'
param environment = 'dev'
param sqlAdminLogin = 'sqladmin'
param sqlAdminPassword = '<STRONG_PASSWORD>'
```

### Step 2 — Deploy everything

```bash
chmod +x infra/deploy.sh
./infra/deploy.sh rg-expense-tracker-dev eastus
```

The script:
1. Logs you into Azure (browser prompt if needed)
2. Creates the resource group
3. Deploys the Bicep template (App Service Plan + Web App + SQL + Key Vault)
4. Performs a second deployment pass to grant the Web App's managed identity access to Key Vault
5. Publishes the .NET app as a ZIP deployment

After completion you'll see:

```
============================================================
  Deployment complete!
  API URL    : https://myapp-api-abc123.azurewebsites.net
  Swagger UI : https://myapp-api-abc123.azurewebsites.net/swagger
  Health     : https://myapp-api-abc123.azurewebsites.net/health
============================================================
```

### Step 3 — Set up CI/CD (GitHub Actions)

1. In Azure Portal, go to your Web App → **Get publish profile** → download the file
2. In your GitHub repo: **Settings** → **Secrets and variables** → **Actions** → add:
   - `AZURE_WEBAPP_NAME` — your Web App name (e.g. `myapp-api-abc123`)
   - `AZURE_WEBAPP_PUBLISH_PROFILE` — paste the full publish profile XML
3. Push to `main` — the pipeline builds, tests, and deploys automatically

---

## Power Apps Setup

See [`powerapps/POWERAPPS_SETUP.md`](powerapps/POWERAPPS_SETUP.md) for the complete guide including:

- Custom Connector setup (import from Swagger)
- 3-screen Canvas App with full Power Fx formulas
- Error handling patterns
- Export / Import instructions

---

## API Reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/expenses?userId={id}` | List all expenses (optional `category` filter) |
| `GET` | `/expenses/{id}` | Get single expense by GUID |
| `POST` | `/expenses` | Create a new expense |
| `PUT` | `/expenses/{id}` | Update an existing expense |
| `DELETE` | `/expenses/{id}` | Delete an expense |
| `GET` | `/expenses/summary?userId={id}` | Spending totals by category + grand total |
| `GET` | `/health` | Health check (EF Core DB connectivity) |
| `GET` | `/swagger` | Interactive API documentation |

### Category Values

| Value | Name |
|-------|------|
| `1` | Food |
| `2` | Travel |
| `3` | Office |
| `4` | Entertainment |
| `5` | Health |
| `6` | Other |

---

## Interview Talking Points

### 1. Clean Architecture with Strict Dependency Flow

> "I implemented Clean Architecture with four layers: Domain, Application, Infrastructure, and API. The key design decision is that **dependencies only point inward** — Domain has zero external dependencies, Application depends only on Domain, and Infrastructure contains all the messy EF Core details. This means I can swap SQL Server for Cosmos DB or MongoDB by changing only the Infrastructure project, without touching the business logic."

### 2. Repository + Unit of Work Patterns

> "The `IExpenseRepository` and `IUnitOfWork` interfaces live in the Domain layer, and their EF Core implementations live in Infrastructure. The service layer calls `_unitOfWork.SaveChangesAsync()` once after all mutations, giving us a single commit boundary. This also makes unit testing trivial — I just swap in an in-memory implementation of `IUnitOfWork` without ever touching a real database."

### 3. Global Exception Handling with RFC 7807 Problem Details

> "Rather than scattering try-catch blocks everywhere, I have a single `ExceptionHandlingMiddleware` that catches all unhandled exceptions and converts them to structured Problem Details responses (RFC 7807). `NotFoundException` → 404, `ValidationException` → 422 Unprocessable Entity, anything else → 500. Clients always get a consistent `application/problem+json` payload with `title`, `detail`, and `instance` fields — no stack traces leak to production."

### 4. Azure Bicep IaC with Managed Identity + Key Vault

> "The entire Azure infrastructure is defined as code in `main.bicep`. The Web App gets a **system-assigned managed identity**, which is then granted the Key Vault Secrets User RBAC role via a Bicep role assignment. The connection string is stored as a Key Vault secret, and the App Setting references it using `@Microsoft.KeyVault(SecretUri=...)` syntax — so the raw connection string never exists in App Settings, never in source control, and the app retrieves it transparently at runtime without any SDK code changes."

### 5. Async/Await + Cancellation Tokens Throughout

> "Every I/O-bound operation accepts a `CancellationToken` — from the controller action methods all the way down through the service layer into the EF Core queries. When a client disconnects mid-request, ASP.NET Core cancels the token, EF Core abandons the in-flight query, and the thread is freed immediately rather than finishing pointless database work. This makes the API significantly more resilient under high-cancellation scenarios like slow mobile clients."
