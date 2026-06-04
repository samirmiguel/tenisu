#!/usr/bin/env bash
set -euo pipefail

# ─── Configuration ────────────────────────────────────────────────────────────
RESOURCE_GROUP="rg-tenisu"
LOCATION="northeurope"
ACR_NAME="tenisuacr"
IMAGE_NAME="tenisu-api"
IMAGE_TAG="latest"
CONTAINER_APP_NAME="tenisu-api"
CONTAINER_APP_ENV="tenisu-env"
# ──────────────────────────────────────────────────────────────────────────────

echo "==> Logging in to Azure..."
az login

echo "==> Registering required Azure resource providers..."
az provider register --namespace Microsoft.ContainerRegistry --wait
az provider register --namespace Microsoft.App --wait
az provider register --namespace Microsoft.OperationalInsights --wait

echo "==> Creating resource group '$RESOURCE_GROUP' in '$LOCATION'..."
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION"

echo "==> Creating Azure Container Registry '$ACR_NAME'..."
az acr create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$ACR_NAME" \
  --sku Basic \
  --admin-enabled true

echo "==> Retrieving ACR login server..."
ACR_LOGIN_SERVER=$(az acr show \
  --name "$ACR_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query loginServer \
  --output tsv)

echo "==> Logging in to ACR '$ACR_LOGIN_SERVER'..."
az acr login --name "$ACR_NAME"

echo "==> Building Docker image locally..."
docker build -t "${ACR_LOGIN_SERVER}/${IMAGE_NAME}:${IMAGE_TAG}" .

echo "==> Pushing image to '$ACR_LOGIN_SERVER'..."
docker push "${ACR_LOGIN_SERVER}/${IMAGE_NAME}:${IMAGE_TAG}"

echo "==> Retrieving ACR credentials..."
ACR_USERNAME=$(az acr credential show --name "$ACR_NAME" --query username --output tsv)
ACR_PASSWORD=$(az acr credential show --name "$ACR_NAME" --query "passwords[0].value" --output tsv)

echo "==> Creating Container Apps Environment '$CONTAINER_APP_ENV'..."
az containerapp env create \
  --name "$CONTAINER_APP_ENV" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION"

echo "==> Deploying Container App '$CONTAINER_APP_NAME'..."
az containerapp create \
  --name "$CONTAINER_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --environment "$CONTAINER_APP_ENV" \
  --image "${ACR_LOGIN_SERVER}/${IMAGE_NAME}:${IMAGE_TAG}" \
  --registry-server "$ACR_LOGIN_SERVER" \
  --registry-username "$ACR_USERNAME" \
  --registry-password "$ACR_PASSWORD" \
  --target-port 8080 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 3 \
  --cpu 0.5 \
  --memory 1.0Gi

echo "==> Deployment complete."
PUBLIC_URL=$(az containerapp show \
  --name "$CONTAINER_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query properties.configuration.ingress.fqdn \
  --output tsv)

echo ""
echo "Public URL: https://${PUBLIC_URL}"
echo "Swagger UI: https://${PUBLIC_URL}/swagger"
echo "Health:     https://${PUBLIC_URL}/health"
