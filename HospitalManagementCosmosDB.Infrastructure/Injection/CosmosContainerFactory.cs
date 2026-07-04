using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace HospitalManagementCosmosDB.Infrastructure.Injection
{
    public class CosmosContainerFactory
    {
        private readonly CosmosClient _client;
        private readonly CosmosDbOptions _options;

        public CosmosContainerFactory(CosmosClient client, IOptions<CosmosDbOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public Container GetContainer(string containerId)
        {
            return _client.GetContainer(_options.DatabaseId, containerId);
        }
    }
}
