#!/bin/bash

# Local test runner script for Aspire with Cosmos DB Emulator
# This script sets up the proper environment and runs the tests locally

set -e

echo "🧪 Running Aspire tests locally with Cosmos DB Emulator..."

# Ensure we're in the right directory
cd "$(dirname "$0")/.."

# Set environment variables for local testing
export ASPNETCORE_ENVIRONMENT=Development
export ASPIRE_ALLOW_UNSECURED_TRANSPORT=true
export DOTNET_DASHBOARD_OTLP_ENDPOINT_URL=""

echo "📦 Building solution..."
dotnet build --configuration Release

echo "🔍 Running tests with extended timeout..."
dotnet test AspireWithCosmos.Tests \
  --no-build \
  --configuration Release \
  --verbosity normal \
  --logger "console;verbosity=detailed" \
  -- RunConfiguration.TestSessionTimeout=600000

echo "✅ Tests completed!"
