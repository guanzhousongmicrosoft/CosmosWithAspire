# GitHub Actions CI/CD

This repository includes several GitHub Actions workflows for building, testing, and maintaining the Aspire application with Cosmos DB Emulator.

## Workflows

### 1. `ci.yml` - Basic CI Pipeline
- **Triggers**: Push to main/develop, Pull requests to main
- **Features**:
  - .NET 9.0 setup
  - Solution build
  - Test execution
  - Test result reporting

### 2. `build-and-test.yml` - Comprehensive CI Pipeline
- **Triggers**: Push to main/develop, Pull requests to main
- **Features**:
  - .NET package caching
  - Code coverage collection
  - Codecov integration
  - Security vulnerability scanning
  - Detailed test reporting

### 3. `aspire-tests.yml` - Aspire-Specific Testing
- **Triggers**: Push to main/develop, Pull requests to main
- **Features**:
  - Docker daemon setup for Cosmos DB Emulator
  - Pre-pulling Docker images
  - Extended timeouts for container startup
  - Docker cleanup after tests

## Requirements

### Repository Setup
1. Enable Actions in your GitHub repository
2. (Optional) Add `CODECOV_TOKEN` secret for code coverage reporting

### Dependencies
- .NET 9.0 SDK
- Docker (for Cosmos DB Emulator)
- Aspire workload

## Running Tests Locally

### Prerequisites
```bash
# Install .NET 9.0
dotnet --version  # Should show 9.0.x

# Install Aspire workload
dotnet workload install aspire

# Ensure Docker is running
docker --version
```

### Build and Test
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests
dotnet test AspireWithCosmos.Tests --configuration Release --verbosity normal
```

## Troubleshooting

### Common Issues

1. **Docker Permission Issues**
   ```bash
   sudo chmod 666 /var/run/docker.sock
   ```

2. **Cosmos DB Emulator Startup Timeout**
   - The workflows include extended timeouts
   - Emulator can take 30+ seconds to fully start

3. **Package Vulnerabilities**
   - The security scan will fail if vulnerable packages are detected
   - Update packages: `dotnet list package --outdated`

### Workflow Status Badges

Add these to your main README.md:

```markdown
![CI](https://github.com/your-username/your-repo/workflows/CI/badge.svg)
![Build and Test](https://github.com/your-username/your-repo/workflows/Build%20and%20Test/badge.svg)
![Aspire Tests](https://github.com/your-username/your-repo/workflows/Aspire%20Tests/badge.svg)
```

## Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Development` for testing
- `DOCKER_HOST`: Docker daemon socket path
- `ASPIRE_ALLOW_UNSECURED_TRANSPORT`: Allow HTTP for local testing

### Timeouts
- Overall job timeout: 30 minutes
- Test session timeout: 10 minutes
- Suitable for Cosmos DB Emulator startup time

## Code Coverage

The `build-and-test.yml` workflow collects code coverage and uploads to Codecov. To set this up:

1. Create account at [codecov.io](https://codecov.io)
2. Add your repository
3. Add `CODECOV_TOKEN` to repository secrets (if private repo)

## Security Scanning

The workflows include vulnerability scanning for NuGet packages. This will:
- Check for known vulnerable packages
- Fail the build if critical vulnerabilities are found
- Recommend package updates

## Best Practices

1. **Branch Protection**: Set up branch protection rules requiring status checks
2. **Test Coverage**: Aim for >80% code coverage
3. **Security**: Regularly update dependencies
4. **Performance**: Monitor test execution times
5. **Docker**: Keep base images updated

## Next Steps

1. Configure branch protection rules
2. Set up Codecov integration
3. Add more comprehensive tests
4. Consider adding deployment workflows
5. Set up automated dependency updates with Dependabot
