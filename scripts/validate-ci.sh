#!/bin/bash

# Local validation script for GitHub Actions workflows
# This script simulates the CI environment locally

set -e

echo "🚀 Starting local CI validation..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Check prerequisites
echo "🔍 Checking prerequisites..."

# Check .NET
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    print_status ".NET SDK found: $DOTNET_VERSION"
    
    if [[ $DOTNET_VERSION == 9.* ]]; then
        print_status ".NET 9.0 detected"
    else
        print_warning ".NET version is $DOTNET_VERSION, expected 9.0.x"
    fi
else
    print_error ".NET SDK not found"
    exit 1
fi

# Check Docker
if command -v docker &> /dev/null; then
    DOCKER_VERSION=$(docker --version)
    print_status "Docker found: $DOCKER_VERSION"
    
    # Check if Docker daemon is running
    if docker info &> /dev/null; then
        print_status "Docker daemon is running"
    else
        print_error "Docker daemon is not running"
        exit 1
    fi
else
    print_error "Docker not found"
    exit 1
fi

# Check Aspire workload
echo "🔍 Checking Aspire workload..."
if dotnet workload list | grep -q "aspire"; then
    print_status "Aspire workload is installed"
else
    print_warning "Aspire workload not found, installing..."
    dotnet workload install aspire
fi

# Simulate CI steps
echo "🏗️  Starting build process..."

# Step 1: Restore dependencies
echo "📦 Restoring dependencies..."
if dotnet restore --verbosity normal; then
    print_status "Dependencies restored successfully"
else
    print_error "Failed to restore dependencies"
    exit 1
fi

# Step 2: Build solution
echo "🔨 Building solution..."
if dotnet build --no-restore --configuration Release --verbosity normal; then
    print_status "Solution built successfully"
else
    print_error "Failed to build solution"
    exit 1
fi

# Step 3: Check for vulnerable packages
echo "🔐 Checking for vulnerable packages..."
VULNERABLE_OUTPUT=$(dotnet list package --vulnerable --include-transitive 2>&1)
if echo "$VULNERABLE_OUTPUT" | grep -q "has the following vulnerable packages"; then
    print_warning "Vulnerable packages found:"
    echo "$VULNERABLE_OUTPUT"
else
    print_status "No vulnerable packages found"
fi

# Step 4: Run tests
echo "🧪 Running tests..."
if dotnet test AspireWithCosmos.Tests \
    --no-build \
    --configuration Release \
    --verbosity normal \
    --logger "console;verbosity=detailed"; then
    print_status "All tests passed!"
else
    print_error "Tests failed"
    exit 1
fi

# Step 5: Cleanup
echo "🧹 Cleaning up Docker containers..."
RUNNING_CONTAINERS=$(docker ps -q)
if [ ! -z "$RUNNING_CONTAINERS" ]; then
    echo "Stopping running containers..."
    docker stop $RUNNING_CONTAINERS
    print_status "Containers stopped"
fi

# Docker system cleanup
if docker system prune -f &> /dev/null; then
    print_status "Docker system cleaned up"
fi

echo ""
print_status "🎉 Local CI validation completed successfully!"
print_status "Your code is ready for GitHub Actions!"

echo ""
echo "📋 Summary:"
echo "  • .NET version: $DOTNET_VERSION"
echo "  • Docker: Available and running"
echo "  • Build: Success"
echo "  • Tests: Passed"
echo "  • Security: Checked"
echo ""
echo "🚀 You can now safely push to trigger GitHub Actions workflows."
