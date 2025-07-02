using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace AspireWithCosmos.Tests;

public class WebTests
{
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireWithCosmos_AppHost>();
        
        // Configure HTTP client with resilience and SSL bypass for CI
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
            
            clientBuilder.ConfigureHttpClient(httpClient =>
            {
                httpClient.Timeout = TimeSpan.FromMinutes(2);
            });
            
            // Always configure certificate bypass since CI environments can't trust dev certs
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                
                // Bypass SSL certificate validation in test/CI environments
                if (IsRunningInCI() || IsTestEnvironment())
                {
                    handler.ServerCertificateCustomValidationCallback = 
                        (message, cert, chain, errors) => true;
                }
                
                return handler;
            });
        });
        
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        var response = await httpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static bool IsRunningInCI()
    {
        // Check for common CI environment variables
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_BUILDID")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITLAB_CI"));
    }

    private static bool IsTestEnvironment()
    {
        // Check if we're running in a test environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(environment, "Test", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
    }
}
