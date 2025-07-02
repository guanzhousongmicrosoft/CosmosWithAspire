#pragma warning disable ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(options =>
    {
        options.WithLifetime(ContainerLifetime.Persistent);
    });

var database = cosmos.AddCosmosDatabase("tododb");
var container = database.AddContainer("todos", "/UserId");

var apiService = builder.AddProject<Projects.AspireWithCosmos_ApiService>("apiservice")
                        .WithReference(cosmos);

builder.AddProject<Projects.AspireWithCosmos_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
