using Microsoft.Azure.Cosmos;
using NSwag;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddHostedService<DatabaseBootstrapper>();

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "1.0.0",
            Title = "Cosmos Todos",
            Description = "Sample Todo app built using Aspire and Azure Cosmos DB.",
        };
    };
});

builder.AddAzureCosmosClient("cosmos");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/", () => "Todo API is up")
   .Produces(StatusCodes.Status200OK)
   .WithOpenApi(operation =>
{
    operation.Summary = "API Status";
    operation.OperationId = "getAPIStatus";
    return operation;
});

app.MapPost("/todo", async (string todoDescription, CosmosClient cosmosClient) =>
{
    var database = cosmosClient.GetDatabase("tododatabase");
    var incomplete = database.GetContainer("incomplete");
    var newTodo = new Todo(todoDescription, false);
    var result = await incomplete.CreateItemAsync<Todo>(newTodo);
    return result.Resource;
})
   .Produces<Todo>(StatusCodes.Status201Created)
   .WithOpenApi(operation =>
{
    operation.Summary = "Creates a new todo item";
    operation.OperationId = "newTodo";
    return operation;
});

app.MapGet("/todos", async (CosmosClient cosmosClient) =>
{
    var database = cosmosClient.GetDatabase("tododatabase");
    var incomplete = database.GetContainer("incomplete");
    var completed = database.GetContainer("completed");

    var todo = incomplete.GetItemLinqQueryable<Todo>(allowSynchronousQueryExecution: true).ToList();
    var done = completed.GetItemLinqQueryable<Todo>(allowSynchronousQueryExecution: true).ToList();

    return todo;
})
   .Produces<List<Todo>>(StatusCodes.Status200OK)
   .WithOpenApi(operation =>
{
    operation.Summary = "Gets all of the todos in the database";
    operation.OperationId = "getAllTodos";
    return operation;
});

app.MapDefaultEndpoints();

app.Run();

public record Todo(string Description, bool IsComplete = false)
{
    [JsonPropertyName("id")]
    public string id { get; set; } = Guid.NewGuid().ToString();
}

public class DatabaseBootstrapper(CosmosClient cosmosClient) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await cosmosClient.CreateDatabaseIfNotExistsAsync("tododatabase");
        var database = cosmosClient.GetDatabase("tododatabase");
        await database.CreateContainerIfNotExistsAsync(new ContainerProperties("incomplete", "/default"));
        await database.CreateContainerIfNotExistsAsync(new ContainerProperties("completed", "/default"));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}