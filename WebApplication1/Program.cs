using HospitalManagementCosmosDB.Application;
using HospitalManagementCosmosDB.Infrastructure.Injection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

// Uncomment these when JWT/Identity authentication is configured
// app.UseAuthentication();
// app.UseAuthorization();

// Initialize Cosmos DB
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var cosmosOptions = services.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

        var cosmosClient = services.GetRequiredService<CosmosClient>();

        logger.LogInformation("Database: {Database}", cosmosOptions.DatabaseId);

        foreach (var container in cosmosOptions.Containers)
        {
            logger.LogInformation(
                "Container: {Container}, Partition Key: {PartitionKey}",
                container.ContainerId,
                container.PartitionKeyPath
            );
        }

        await CosmosInitializer.InitializeAsync(cosmosClient, cosmosOptions);

        logger.LogInformation("Cosmos DB initialized successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to initialize Cosmos DB.");

        throw;
    }
}

app.MapControllers();

app.Run();
