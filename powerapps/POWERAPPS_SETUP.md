# Power Apps Setup Guide — Expense Tracker

This guide walks you through connecting a Power Apps Canvas App to the Expense Tracker REST API using a Custom Connector.

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Create the Custom Connector](#2-create-the-custom-connector)
3. [Build the Canvas App](#3-build-the-canvas-app)
   - [Screen 1 — Home (Expense Gallery)](#screen-1--home-expense-gallery)
   - [Screen 2 — Add / Edit Expense](#screen-2--add--edit-expense)
   - [Screen 3 — Summary (Chart)](#screen-3--summary-chart)
4. [Power Fx Reference](#4-power-fx-reference)
5. [Error Handling Patterns](#5-error-handling-patterns)
6. [Export & Import the App](#6-export--import-the-app)

---

## 1. Prerequisites

| Requirement | Details |
|-------------|---------|
| Power Apps licence | Microsoft 365 with Power Apps, or a standalone Power Apps plan |
| Deployed API URL | e.g. `https://exptr-api-abc123.azurewebsites.net` |
| Swagger JSON URL | `{API_URL}/swagger/v1/swagger.json` |
| Environment | A Power Platform environment (default environment is fine for dev) |

---

## 2. Create the Custom Connector

### 2a. Open Custom Connectors

1. Go to [make.powerapps.com](https://make.powerapps.com)
2. In the left navigation expand **Data** → **Custom Connectors**
3. Click **+ New custom connector** → **Import an OpenAPI from URL**

### 2b. Import from Swagger

| Field | Value |
|-------|-------|
| Connector name | `ExpenseTrackerAPI` |
| OpenAPI URL | `https://<your-app>.azurewebsites.net/swagger/v1/swagger.json` |

Click **Import** → **Continue**.

### 2c. General Tab

| Field | Value |
|-------|-------|
| Scheme | HTTPS |
| Host | `<your-app>.azurewebsites.net` |
| Base URL | `/` |
| Description | Expense Tracker REST API |

Upload an icon (optional).

### 2d. Security Tab

- Select **No authentication** (the API is open by default)
- *Optional:* If you add `X-API-Key` header auth to the .NET API, choose **API Key** here:
  - Parameter label: `API Key`
  - Parameter name: `X-API-Key`
  - Parameter location: Header

### 2e. Definition Tab

The Swagger import auto-creates all operations. Verify these appear:

| Operation ID | Method | Path |
|-------------|--------|------|
| GetAll | GET | `/expenses` |
| GetSummary | GET | `/expenses/summary` |
| GetById | GET | `/expenses/{id}` |
| Create | POST | `/expenses` |
| Update | PUT | `/expenses/{id}` |
| Delete | DELETE | `/expenses/{id}` |

### 2f. Test Tab

1. Click **Create connector** (bottom right)
2. Click **+ New connection** → test **GetAll** with `userId = testuser`
3. Verify you get a `200 OK` with an empty array `[]`

---

## 3. Build the Canvas App

### Create the App

1. In Power Apps, click **+ Create** → **Blank app** → **Blank canvas app**
2. Name: `Expense Tracker`
3. Format: **Phone** or **Tablet** (your choice)
4. Click **Create**

### Connect the Connector

1. In the left panel click **Data** (cylinder icon) → **Add data**
2. Search for `ExpenseTrackerAPI` → add it
3. The connector appears as `ExpenseTrackerAPI` in your app

### Global Variable Setup (App.OnStart)

Paste this into `App.OnStart`:

```powerfx
// Set the logged-in user ID (replace with real auth if needed)
Set(gUserId, User().Email);

// Load all expenses into a local collection
ClearCollect(
    colExpenses,
    ExpenseTrackerAPI.GetAll({userId: gUserId})
);

// Load summary data
Set(
    gSummary,
    ExpenseTrackerAPI.GetSummary({userId: gUserId})
);

// Initialize edit mode flag
Set(gIsEditMode, false);
Set(gSelectedExpense, Blank());
```

---

### Screen 1 — Home (Expense Gallery)

#### Layout

```
┌──────────────────────────────────────────┐
│  Expense Tracker              [+] [↻]    │
├──────────────────────────────────────────┤
│  ┌────────────────────────────────────┐  │
│  │  Coffee at Starbucks               │  │
│  │  $4.50  •  Food  •  Apr 27, 2026  │  │
│  └────────────────────────────────────┘  │
│  ┌────────────────────────────────────┐  │
│  │  Uber to Airport                   │  │
│  │  $32.00 •  Travel • Apr 26, 2026  │  │
│  └────────────────────────────────────┘  │
│  ...                                     │
└──────────────────────────────────────────┘
```

#### Controls & Formulas

**Screen name:** `HomeScreen`

**Header Label** (`lblTitle`):
- Text: `"My Expenses"`
- Font size: 20, Bold

**Add Button** (`btnAdd`):
- Text: `"+"`
- OnSelect:
```powerfx
Set(gIsEditMode, false);
Set(gSelectedExpense, Blank());
Navigate(EditScreen, ScreenTransition.Cover);
```

**Refresh Button** (`btnRefresh`):
- Text: `"↻"`
- OnSelect:
```powerfx
ClearCollect(
    colExpenses,
    ExpenseTrackerAPI.GetAll({userId: gUserId})
);
Notify("Expenses refreshed", NotificationType.Success);
```

**Expense Gallery** (`galExpenses`):
- Items: `Sort(colExpenses, Date, Descending)`
- Layout: **Title, subtitle, body**

Inside gallery:
- `lblExpenseTitle` → `ThisItem.title`
- `lblExpenseAmount` → `"$" & Text(ThisItem.amount, "[$-en-US]#,##0.00")`
- `lblExpenseCategory` → `ThisItem.categoryName`
- `lblExpenseDate` → `Text(DateValue(Left(ThisItem.date, 10)), "mmm d, yyyy")`
- `btnEditItem` (pencil icon) OnSelect:
```powerfx
Set(gIsEditMode, true);
Set(gSelectedExpense, ThisItem);
Navigate(EditScreen, ScreenTransition.Cover);
```
- `btnDeleteItem` (trash icon) OnSelect:
```powerfx
If(
    Confirm("Delete '" & ThisItem.title & "'?"),
    // Delete via API
    ExpenseTrackerAPI.Delete(ThisItem.id);
    // Remove from local collection
    Remove(colExpenses, ThisItem);
    Notify("Expense deleted", NotificationType.Success),
    // User cancelled — do nothing
    false
)
```

**Summary Button** (`btnSummary`):
- Text: `"View Summary"`
- OnSelect:
```powerfx
Set(
    gSummary,
    ExpenseTrackerAPI.GetSummary({userId: gUserId})
);
Navigate(SummaryScreen, ScreenTransition.Cover);
```

---

### Screen 2 — Add / Edit Expense

#### Layout

```
┌──────────────────────────────────────────┐
│  ← Back        Add Expense               │
├──────────────────────────────────────────┤
│  Title *                                 │
│  ┌────────────────────────────────────┐  │
│  │ e.g. Coffee at Starbucks           │  │
│  └────────────────────────────────────┘  │
│  Amount *            Category *          │
│  ┌──────────────┐    ┌────────────────┐  │
│  │ 0.00         │    │ Food        ▼  │  │
│  └──────────────┘    └────────────────┘  │
│  Date *                                  │
│  ┌────────────────────────────────────┐  │
│  │ 04/27/2026                         │  │
│  └────────────────────────────────────┘  │
│  Notes (optional)                        │
│  ┌────────────────────────────────────┐  │
│  │                                    │  │
│  └────────────────────────────────────┘  │
│  ┌─────────────┐  ┌───────────────────┐  │
│  │   Cancel    │  │       Save        │  │
│  └─────────────┘  └───────────────────┘  │
└──────────────────────────────────────────┘
```

#### Controls & Formulas

**Screen name:** `EditScreen`

**Screen.OnVisible**:
```powerfx
// Pre-populate form when editing
If(
    gIsEditMode,
    UpdateContext({
        ctxTitle: gSelectedExpense.title,
        ctxAmount: Text(gSelectedExpense.amount),
        ctxCategory: gSelectedExpense.categoryName,
        ctxDate: DateValue(Left(gSelectedExpense.date, 10)),
        ctxNotes: gSelectedExpense.notes
    }),
    UpdateContext({
        ctxTitle: "",
        ctxAmount: "",
        ctxCategory: "Food",
        ctxDate: Today(),
        ctxNotes: ""
    })
)
```

**Header Label**:
- Text: `If(gIsEditMode, "Edit Expense", "Add Expense")`

**Back Button** (`btnBack`):
- OnSelect: `Navigate(HomeScreen, ScreenTransition.UnCover)`

**Title Input** (`inpTitle`):
- Default: `ctxTitle`
- OnChange: `UpdateContext({ctxTitle: Self.Text})`

**Amount Input** (`inpAmount`):
- Default: `ctxAmount`
- Format: Numbers only
- OnChange: `UpdateContext({ctxAmount: Self.Text})`

**Category Dropdown** (`ddCategory`):
- Items: `["Food","Travel","Office","Entertainment","Health","Other"]`
- Default: `ctxCategory`
- OnChange: `UpdateContext({ctxCategory: Self.SelectedText.Value})`

**Date Picker** (`dpDate`):
- DefaultDate: `ctxDate`
- OnChange: `UpdateContext({ctxDate: Self.SelectedDate})`

**Notes Input** (`inpNotes`):
- Default: `ctxNotes`
- Mode: Multiline
- OnChange: `UpdateContext({ctxNotes: Self.Text})`

**Cancel Button** (`btnCancel`):
- OnSelect: `Navigate(HomeScreen, ScreenTransition.UnCover)`

**Save Button** (`btnSave`):
- OnSelect (full formula):

```powerfx
// Client-side validation
If(
    IsBlank(ctxTitle),
    Notify("Title is required", NotificationType.Error);
    false,

    IsBlank(ctxAmount) || !IsNumeric(ctxAmount) || Value(ctxAmount) <= 0,
    Notify("Amount must be a positive number", NotificationType.Error);
    false,

    // Validation passed — call API
    If(
        gIsEditMode,
        // ── UPDATE ──────────────────────────────────────────────
        With(
            {
                result: ExpenseTrackerAPI.Update(
                    gSelectedExpense.id,
                    {
                        title: ctxTitle,
                        amount: Value(ctxAmount),
                        category: Switch(
                            ctxCategory,
                            "Food", 1,
                            "Travel", 2,
                            "Office", 3,
                            "Entertainment", 4,
                            "Health", 5,
                            6
                        ),
                        date: Text(ctxDate, "yyyy-mm-dd") & "T00:00:00Z",
                        notes: ctxNotes
                    }
                )
            },
            // Replace in local collection
            UpdateIf(
                colExpenses,
                id = gSelectedExpense.id,
                {
                    title: result.title,
                    amount: result.amount,
                    categoryName: result.categoryName,
                    date: result.date,
                    notes: result.notes
                }
            );
            Notify("Expense updated!", NotificationType.Success);
            Navigate(HomeScreen, ScreenTransition.UnCover)
        ),

        // ── CREATE ──────────────────────────────────────────────
        With(
            {
                newExpense: ExpenseTrackerAPI.Create(
                    {
                        title: ctxTitle,
                        amount: Value(ctxAmount),
                        category: Switch(
                            ctxCategory,
                            "Food", 1,
                            "Travel", 2,
                            "Office", 3,
                            "Entertainment", 4,
                            "Health", 5,
                            6
                        ),
                        date: Text(ctxDate, "yyyy-mm-dd") & "T00:00:00Z",
                        userId: gUserId,
                        notes: ctxNotes
                    }
                )
            },
            // Add to local collection
            Collect(colExpenses, newExpense);
            Notify("Expense added!", NotificationType.Success);
            Navigate(HomeScreen, ScreenTransition.UnCover)
        )
    )
)
```

---

### Screen 3 — Summary (Chart)

#### Layout

```
┌──────────────────────────────────────────┐
│  ← Back           Spending Summary       │
├──────────────────────────────────────────┤
│                                          │
│         ╭──────────────╮                 │
│       ╭─┤  Donut Chart ├─╮               │
│       │ ╰──────────────╯ │               │
│       ╰──────────────────╯               │
│                                          │
│  ┌────────────────────────────────────┐  │
│  │  Grand Total            $1,234.56  │  │
│  └────────────────────────────────────┘  │
│                                          │
│  Food            $320.00     26%         │
│  Travel          $450.00     36%         │
│  Office          $180.00     15%         │
│  Entertainment    $90.00      7%         │
│  Health          $194.56     16%         │
└──────────────────────────────────────────┘
```

#### Controls & Formulas

**Screen name:** `SummaryScreen`

**Screen.OnVisible**:
```powerfx
Set(
    gSummary,
    ExpenseTrackerAPI.GetSummary({userId: gUserId})
);

// Build a table for the chart
ClearCollect(
    colSummaryChart,
    ForAll(
        ["Food","Travel","Office","Entertainment","Health","Other"],
        {
            Category: Value,
            Amount: If(
                !IsBlank(LookUp(colExpenses, categoryName = Value, amount)),
                Sum(Filter(colExpenses, categoryName = Value), amount),
                0
            )
        }
    )
);
```

**Back Button**:
- OnSelect: `Navigate(HomeScreen, ScreenTransition.UnCover)`

**Pie / Donut Chart** (`chartSummary`):
- Type: **Donut chart** (Insert → Charts → Donut chart)
- Items: `colSummaryChart`
- ItemsLabels: `"Category"`
- ItemsValues: `"Amount"`
- Legend: Bottom

**Grand Total Card** (`lblGrandTotal`):
- Text: `"Grand Total: $" & Text(gSummary.grandTotal, "[$-en-US]#,##0.00")`
- Font size: 18, Bold

**Summary Gallery** (`galSummary`):
- Items: `colSummaryChart`
- Template contains:
  - Category label: `ThisItem.Category`
  - Amount label: `"$" & Text(ThisItem.Amount, "#,##0.00")`
  - Percentage label:
  ```powerfx
  If(
      gSummary.grandTotal > 0,
      Text(ThisItem.Amount / gSummary.grandTotal * 100, "0") & "%",
      "0%"
  )
  ```

---

## 4. Power Fx Reference

### Load / Refresh All Expenses

```powerfx
ClearCollect(
    colExpenses,
    ExpenseTrackerAPI.GetAll({userId: gUserId})
);
```

### Get Single Expense

```powerfx
Set(
    gCurrentExpense,
    ExpenseTrackerAPI.GetById(someGuid)
);
```

### Create Expense

```powerfx
Collect(
    colExpenses,
    ExpenseTrackerAPI.Create({
        title: "New Expense",
        amount: 25.00,
        category: 1,           // 1=Food, 2=Travel, 3=Office, 4=Entertainment, 5=Health, 6=Other
        date: "2026-04-27T00:00:00Z",
        userId: gUserId,
        notes: "Optional notes"
    })
);
```

### Update Expense

```powerfx
With(
    {updated: ExpenseTrackerAPI.Update(expenseId, {
        title: "Updated Title",
        amount: 30.00,
        category: 2,
        date: "2026-04-27T00:00:00Z",
        notes: ""
    })},
    UpdateIf(colExpenses, id = expenseId, {
        title: updated.title,
        amount: updated.amount
    })
);
```

### Delete Expense

```powerfx
ExpenseTrackerAPI.Delete(expenseId);
Remove(colExpenses, LookUp(colExpenses, id = expenseId));
```

### Get Summary

```powerfx
Set(gSummary, ExpenseTrackerAPI.GetSummary({userId: gUserId}));
// Access: gSummary.grandTotal, gSummary.byCategory
```

---

## 5. Error Handling Patterns

Wrap every API call in an `IfError()` for production-quality error handling:

```powerfx
IfError(
    // Happy path
    ClearCollect(
        colExpenses,
        ExpenseTrackerAPI.GetAll({userId: gUserId})
    );
    Notify("Loaded " & CountRows(colExpenses) & " expenses", NotificationType.Success),

    // Error path — FirstError is set by Power Apps
    Notify(
        "Failed to load expenses: " & FirstError.Message,
        NotificationType.Error
    )
);
```

---

## 6. Export & Import the App

### Export

1. Go to [make.powerapps.com](https://make.powerapps.com) → **Apps**
2. Select **Expense Tracker** → **⋮** → **Export package**
3. Fill in Name / Description / Version
4. Under **Review Package Content**, set the connector to **Create as new**
5. Click **Export** — downloads `ExpenseTracker.zip` (`.msapp` inside)

### Import

1. Go to target environment → **Apps** → **Import canvas app**
2. Upload the exported `.zip`
3. Resolve conflicts (Create new / Use existing for the connector)
4. Update the connector host URL if deploying to a different API endpoint:
   - Data → Custom Connectors → `ExpenseTrackerAPI` → Edit → General tab → Host
5. Click **Import**
6. Open the app, go to **Data** and refresh the connector connection

### Share the App

1. **Apps** → **⋮** → **Share**
2. Enter user names or AAD groups
3. Check **Co-owner** for developers, leave unchecked for users
4. Users also need access to the **ExpenseTrackerAPI** connection — Power Apps will prompt them on first launch
