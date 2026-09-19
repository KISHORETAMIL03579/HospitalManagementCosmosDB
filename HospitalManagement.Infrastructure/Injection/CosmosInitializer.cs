using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Infrastructure.Injection
{
    public static class CosmosInitializer
    {
        public static async Task InitializeAsync(
            CosmosClient client,
            CosmosDbOptions options,
            ILogger? logger = null
        )
        {
            var dbResponse = await client.CreateDatabaseIfNotExistsAsync(options.DatabaseId);
            var database = dbResponse.Database;

            foreach (var item in options.Containers)
            {
                bool created = false;

                for (int attempt = 1; attempt <= 10; attempt++)
                {
                    try
                    {
                        var containerProperties = new ContainerProperties
                        {
                            Id = item.ContainerId,
                            PartitionKeyPath = item.PartitionKeyPath,
                        };

                        if (item.Throughput.HasValue && item.Throughput.Value > 0)
                        {
                            await database.CreateContainerIfNotExistsAsync(
                                containerProperties,
                                throughput: item.Throughput.Value
                            );
                        }
                        else
                        {
                            await database.CreateContainerIfNotExistsAsync(containerProperties);
                        }

                        logger?.LogInformation(
                            "Ensured Cosmos DB container: {ContainerId} with Partition Key {PartitionKeyPath}",
                            item.ContainerId,
                            item.PartitionKeyPath
                        );

                        created = true;
                        break;
                    }
                    catch (CosmosException ex)
                        when (ex.StatusCode == HttpStatusCode.ServiceUnavailable
                            || ex.StatusCode == HttpStatusCode.TooManyRequests
                            || ex.StatusCode == HttpStatusCode.RequestTimeout
                        )
                    {
                        logger?.LogWarning(
                            ex,
                            "Attempt {Attempt}/10 failed for container {ContainerId} (Status: {StatusCode}). Retrying in 3 seconds...",
                            attempt,
                            item.ContainerId,
                            ex.StatusCode
                        );

                        await Task.Delay(3000);
                    }
                }

                if (!created)
                {
                    logger?.LogWarning(
                        "Skipping container creation for {ContainerId} after 10 attempts. Cosmos DB emulator may still be initializing.",
                        item.ContainerId
                    );
                }
            }
        }
    }
}
