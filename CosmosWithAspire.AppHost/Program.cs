var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos");

var apiService = builder.AddProject<Projects.CosmosWithAspire_ApiService>("apiservice")
                        .WithReference(cosmos);

builder.AddProject<Projects.CosmosWithAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
