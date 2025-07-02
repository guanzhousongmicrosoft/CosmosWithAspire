#pragma warning disable ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var builder = DistributedApplication.CreateBuilder(args);

// Check if running in CI/test environment
var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) || 
           !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(options =>
    {
        options.WithLifetime(ContainerLifetime.Persistent);
        
        // In CI environments, ensure proper networking
        if (isCI)
        {
            options.WithContainerRuntimeArgs("--network=bridge");
        }
    });

var database = cosmos.AddCosmosDatabase("tododb");
var container = database.AddContainer("todos", "/UserId");

var apiService = builder.AddProject<Projects.AspireWithCosmos_ApiService>("apiservice")
                        .WithReference(cosmos);

var webApp = builder.AddProject<Projects.AspireWithCosmos_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

// In CI environments, prefer HTTP over HTTPS to avoid certificate issues
if (isCI)
{
    webApp.WithHttpEndpoint(port: 5000, name: "http");
}

builder.Build().Run();
