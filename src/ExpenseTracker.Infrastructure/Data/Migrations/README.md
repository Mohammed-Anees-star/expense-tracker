# EF Core Migrations

Migrations are generated via the EF Core CLI from the solution root.

## Generate initial migration

```bash
dotnet ef migrations add InitialCreate \
  --project src/ExpenseTracker.Infrastructure \
  --startup-project src/ExpenseTracker.API \
  --output-dir Data/Migrations
```

## Apply migrations

```bash
dotnet ef database update \
  --project src/ExpenseTracker.Infrastructure \
  --startup-project src/ExpenseTracker.API
```

## Notes

- The application calls `db.Database.Migrate()` on startup automatically.
- For local development the connection string in `appsettings.Development.json` points to a SQLite file (`expense-tracker.db`).
- For Azure, the connection string is sourced from Key Vault via an App Service Key Vault reference.
