using HospitalManagementCosmosDB.Infrastructure.Injection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// DI
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Get Cosmos options and client once
    var cosmosOptions = services.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
    var cosmosClient = services.GetRequiredService<CosmosClient>();

    // Log info
    Console.WriteLine($"DB = {cosmosOptions.DatabaseId}");

    // Initialize database and container
    foreach (var c in cosmosOptions.Containers)
    {
        Console.WriteLine($"Container = {c.ContainerId}, PK = {c.PartitionKeyPath}");
    }

    await CosmosInitializer.InitializeAsync(cosmosClient, cosmosOptions);
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
