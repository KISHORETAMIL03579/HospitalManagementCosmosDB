//using Microsoft.Azure.Cosmos;

//namespace HospitalManagementCosmosDB.Infrastructure.Data
//{
//    public class CosmosDB
//    {
//        private readonly CosmosClient _client;

//        public CosmosDB(CosmosClient client)
//        {
//            _client = client ?? throw new ArgumentNullException(nameof(client));
//        }
//        public static async Task<Container> EnsureContainerExistsAsync(
//            CosmosClient client,
//            string databaseId,
//            string containerId,
//            string partitionKeyPath = "/id")
//        {
//            // Ensure database
//            Database database;
//            try
//            {
//                database = client.GetDatabase(databaseId);
//                await database.ReadAsync();
//                Console.WriteLine($"Database '{databaseId}' exists.");
//            }
//            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
//            {
//                database = (await client.CreateDatabaseAsync(databaseId)).Database;
//                Console.WriteLine($"Database '{databaseId}' created.");
//            }

//            // Ensure container
//            Container container;
//            try
//            {
//                container = database.GetContainer(containerId);
//                await container.ReadContainerAsync();
//                Console.WriteLine($"Container '{containerId}' exists.");
//            }
//            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
//            {
//                container = (await database.CreateContainerAsync(containerId, partitionKeyPath)).Container;
//                Console.WriteLine($"Container '{containerId}' created.");
//            }

//            return container;
//        }
//    }
//}
