using System.Net;
using HospitalManagementCosmosDB.Domain.Entities;
using HospitalManagementCosmosDB.Infrastructure.Injection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace HospitalManagementCosmosDB.Infrastructure.Repository
{
    public class IdempotencyRepository
    {
        private readonly Container _container;
        private readonly ILogger<IdempotencyRepository> _logger;

        public IdempotencyRepository(
            CosmosContainerFactory factory,
            ILogger<IdempotencyRepository> logger
        )
        {
            _container = factory.GetContainer("IdempotencyKeys");
            _logger = logger;
        }

        public async Task<Idempotency?> GetAsync(
            string key,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var response = await _container.ReadItemAsync<Idempotency>(
                    id: key,
                    partitionKey: new PartitionKey(key),
                    cancellationToken: cancellationToken
                );

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Idempotency key '{Key}' not found.", key);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving idempotency key '{Key}'.", key);

                throw;
            }
        }

        public async Task SaveAsync(
            Idempotency record,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                await _container.CreateItemAsync(
                    item: record,
                    partitionKey: new PartitionKey(record.Id),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Idempotency key '{Key}' saved successfully.", record.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving idempotency key '{Key}'.", record.Id);

                throw;
            }
        }
    }
}
