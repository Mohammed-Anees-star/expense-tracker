using './main.bicep'

// =============================================================================
// Expense Tracker — Bicep Parameter File
// Replace all placeholder values before deploying.
// =============================================================================

// Short prefix used in resource names (3-12 lowercase alphanumeric chars)
param baseName = 'exptr'

// Azure region  (e.g. 'eastus', 'westeurope', 'australiaeast')
param location = 'eastus'

// Environment tier
param environment = 'dev'

// SQL Server administrator credentials
// IMPORTANT: use a strong password and store it securely (e.g. in a CI secret)
param sqlAdminLogin = 'sqladmin'
param sqlAdminPassword = '<REPLACE_WITH_STRONG_PASSWORD>'

// Leave empty on first deploy; fill in with the output webAppPrincipalIdOutput
// value from the first deployment and redeploy to wire up Key Vault RBAC.
param webAppPrincipalId = ''
