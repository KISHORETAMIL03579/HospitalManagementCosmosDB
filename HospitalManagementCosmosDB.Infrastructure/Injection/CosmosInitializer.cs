using Microsoft.Azure.Cosmos;

namespace HospitalManagementCosmosDB.Infrastructure.Injection
{
    public class CosmosInitializer
    {
        public static async Task InitializeAsync(CosmosClient client, CosmosDbOptions options)
        {
            //var db = await client.CreateDatabaseIfNotExistsAsync(options.DatabaseId);
            //foreach (var item in options.Containers)
            //{
            //    await db.Database.CreateContainerIfNotExistsAsync(id: item.ContainerId, partitionKeyPath: item.PartitionKeyPath, throughput: 400);
            //    Console.WriteLine($"Ensured container: {item.ContainerId}");
            //}

            //var db = await client.CreateDatabaseIfNotExistsAsync(options.DatabaseId);

            //foreach (var item in options.Containers)
            //{
            //    await RetryAsync(async () =>
            //    {
            //        await db.Database.CreateContainerIfNotExistsAsync(
            //            id: item.ContainerId,
            //            partitionKeyPath: item.PartitionKeyPath,
            //            throughput: 400);

            //        Console.WriteLine($"Ensured container: {item.ContainerId}");
            //    });
            //}

            var db = await client.CreateDatabaseIfNotExistsAsync(options.DatabaseId);
            foreach (var item in options.Containers)
            {
                bool created = false;

                for (int i = 1; i <= 10; i++)
                {
                    try
                    {
                        await db.Database.CreateContainerIfNotExistsAsync(
                            id: item.ContainerId,
                            partitionKeyPath: item.PartitionKeyPath,
                            throughput: 400
                        );

                        Console.WriteLine($"Ensured container: {item.ContainerId}");
                        created = true;
                        break;
                    }
                    catch (CosmosException ex)
                    {
                        Console.WriteLine(
                            $"[WARN] Attempt {i} failed for {item.ContainerId}: {ex.StatusCode}"
                        );

                        await Task.Delay(3000);
                    }
                }

                if (!created)
                {
                    Console.WriteLine(
                        $"[WARN] Skipping container {item.ContainerId}. Emulator still initializing."
                    );
                }
            }
        }

        private static async Task RetryAsync(Func<Task> action, int retries = 5)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (CosmosException ex)
                    when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                        || ex.StatusCode == System.Net.HttpStatusCode.NotFound
                    )
                {
                    await Task.Delay(2000);
                }
            }

            throw new Exception("Cosmos DB Emulator is not ready");
        }
    }
}
