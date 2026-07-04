using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace HospitalManagementCosmosDB.Infrastructure.Injection
{
    public class CosmosContainerFactory
    {
        private readonly CosmosClient _client;
        private readonly string _databaseId;

        public CosmosContainerFactory(CosmosClient client, IOptions<CosmosDbOptions> options)
        {
            _client = client;
            _databaseId = options.Value.DatabaseId;
        }

        public Container GetContainer(string containerId)
        {
            return _client.GetContainer(_databaseId, containerId);
        }
    }
}
