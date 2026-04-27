# Postman Collection — Expense Tracker API

## Import

1. Open Postman → **Import** → select `ExpenseTracker.postman_collection.json`
2. The collection imports with two variables pre-set:
   - `baseUrl` → `http://localhost:5000` (local dev)
   - `expenseId` → `1` (auto-updated after Create)

## Switch to Azure

1. In the collection, click **Variables**
2. Change `baseUrl` to your deployed App Service URL, e.g.:  
   `https://expense-tracker-api.azurewebsites.net`

## Recommended Run Order

Run requests in this order for a full happy-path test:

1. **Health Check** — confirm API is up
2. **Create Expense** — auto-saves the new `id` into `{{expenseId}}`
3. **GET All Expenses** — see the list
4. **GET Expense by ID** — fetch the one you just created
5. **GET Expense Summary** — check totals by category
6. **Update Expense** — modify it
7. **Delete Expense** — clean up

## Category Values (enum int)

| Value | Name          |
|-------|---------------|
| 0     | Food          |
| 1     | Travel        |
| 2     | Office        |
| 3     | Entertainment |
| 4     | Health        |
| 5     | Other         |

## Run as Collection (automated)

Use **Collection Runner** or Newman CLI:

```bash
npm install -g newman
newman run ExpenseTracker.postman_collection.json \
  --env-var baseUrl=https://your-app.azurewebsites.net
```
