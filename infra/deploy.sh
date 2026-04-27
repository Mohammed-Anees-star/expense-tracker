#!/usr/bin/env bash
# =============================================================================
# Expense Tracker — Azure Deployment Script
# Usage: ./infra/deploy.sh [resource-group-name] [azure-region]
# =============================================================================

set -euo pipefail

# ── Configuration (override via env vars or CLI args) ───────────────────────
RESOURCE_GROUP="${1:-rg-expense-tracker-dev}"
LOCATION="${2:-eastus}"
PARAM_FILE="$(dirname "$0")/main.bicepparam"
BICEP_FILE="$(dirname "$0")/main.bicep"
SOLUTION_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || dirname "$(dirname "$0")")"
PUBLISH_DIR="${SOLUTION_ROOT}/.publish"

echo "============================================================"
echo "  Expense Tracker — Azure Deployment"
echo "  Resource Group : ${RESOURCE_GROUP}"
echo "  Location       : ${LOCATION}"
echo "============================================================"

# ── 1. Azure Login ──────────────────────────────────────────────────────────
echo ""
echo ">> Step 1/5: Logging in to Azure..."
az account show &>/dev/null || az login

echo "   Subscription: $(az account show --query name -o tsv)"

# ── 2. Create Resource Group ─────────────────────────────────────────────────
echo ""
echo ">> Step 2/5: Ensuring resource group '${RESOURCE_GROUP}' exists..."
az group create \
  --name "${RESOURCE_GROUP}" \
  --location "${LOCATION}" \
  --output none

echo "   Resource group ready."

# ── 3. Deploy Bicep ──────────────────────────────────────────────────────────
echo ""
echo ">> Step 3/5: Deploying Bicep template..."
DEPLOY_OUTPUT=$(az deployment group create \
  --resource-group "${RESOURCE_GROUP}" \
  --template-file "${BICEP_FILE}" \
  --parameters "${PARAM_FILE}" \
  --name "expense-tracker-$(date +%Y%m%d%H%M%S)" \
  --output json)

WEB_APP_URL=$(echo "${DEPLOY_OUTPUT}" | jq -r '.properties.outputs.webAppUrl.value')
SQL_FQDN=$(echo "${DEPLOY_OUTPUT}" | jq -r '.properties.outputs.sqlServerFqdn.value')
KV_NAME=$(echo "${DEPLOY_OUTPUT}" | jq -r '.properties.outputs.keyVaultName.value')
PRINCIPAL_ID=$(echo "${DEPLOY_OUTPUT}" | jq -r '.properties.outputs.webAppPrincipalIdOutput.value')

echo "   Web App URL  : ${WEB_APP_URL}"
echo "   SQL FQDN     : ${SQL_FQDN}"
echo "   Key Vault    : ${KV_NAME}"
echo "   Principal ID : ${PRINCIPAL_ID}"

# ── 3b. Second-pass deploy to wire up Key Vault RBAC ────────────────────────
echo ""
echo ">> Step 3b/5: Re-deploying with managed identity to wire up Key Vault RBAC..."
az deployment group create \
  --resource-group "${RESOURCE_GROUP}" \
  --template-file "${BICEP_FILE}" \
  --parameters "${PARAM_FILE}" \
  --parameters webAppPrincipalId="${PRINCIPAL_ID}" \
  --name "expense-tracker-rbac-$(date +%Y%m%d%H%M%S)" \
  --output none

echo "   Key Vault RBAC configured."

# ── 4. Build & Publish .NET App ──────────────────────────────────────────────
echo ""
echo ">> Step 4/5: Building and publishing .NET 8 app..."
rm -rf "${PUBLISH_DIR}"
dotnet publish "${SOLUTION_ROOT}/src/ExpenseTracker.API/ExpenseTracker.API.csproj" \
  --configuration Release \
  --output "${PUBLISH_DIR}" \
  --runtime linux-x64 \
  --self-contained false \
  /p:UseAppHost=false

# Zip for deployment
ZIP_FILE="${SOLUTION_ROOT}/.publish.zip"
cd "${PUBLISH_DIR}" && zip -r "${ZIP_FILE}" . && cd - > /dev/null
echo "   Publish complete: ${ZIP_FILE}"

# ── 5. Deploy to Azure Web App ───────────────────────────────────────────────
echo ""
echo ">> Step 5/5: Deploying app package to Azure Web App..."

# Retrieve Web App name from the deployment outputs
WEB_APP_NAME=$(echo "${WEB_APP_URL}" | sed 's|https://||' | cut -d'.' -f1)

az webapp deploy \
  --resource-group "${RESOURCE_GROUP}" \
  --name "${WEB_APP_NAME}" \
  --src-path "${ZIP_FILE}" \
  --type zip \
  --output none

echo ""
echo "============================================================"
echo "  Deployment complete!"
echo "  API URL    : ${WEB_APP_URL}"
echo "  Swagger UI : ${WEB_APP_URL}/swagger"
echo "  Health     : ${WEB_APP_URL}/health"
echo "============================================================"

# Clean up
rm -f "${ZIP_FILE}"
