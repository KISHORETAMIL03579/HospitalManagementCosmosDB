using HospitalManagementCosmosDB.Domain.Entities;
using HospitalManagementCosmosDB.Infrastructure.Injection;
using Microsoft.Azure.Cosmos;

namespace HospitalManagementCosmosDB.Infrastructure.Repository
{
    public class IdempotencyRepository
    {
        private readonly Container _container;

        public IdempotencyRepository(CosmosContainerFactory factory)
        {
            _container = factory.GetContainer("IdempotencyKeys");
        }

        public async Task<Idempotency?> GetAsync(string key)
        {
            try
            {
                var response = await _container.ReadItemAsync<Idempotency>(
                    key,
                    new PartitionKey(key)
                );

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task SaveAsync(Idempotency record)
        {
            await _container.CreateItemAsync(record, new PartitionKey(record.Id));
        }
    }
}
