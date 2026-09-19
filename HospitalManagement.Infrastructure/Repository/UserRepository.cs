using System.Net;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Infrastructure.Injection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using User = HospitalManagement.Domain.Entities.User;

namespace HospitalManagement.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly Container _container;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(CosmosContainerFactory factory, ILogger<UserRepository> logger)
        {
            _container = factory.GetContainer("Users");
            _logger = logger;
        }

        public async Task<User?> GetByGoogleSubAsync(
            string googleSub,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var query = _container.GetItemQueryIterator<User>(
                    queryDefinition: new QueryDefinition(
                        "SELECT * FROM c WHERE c.googleSub = @googleSub"
                    ).WithParameter("@googleSub", googleSub),
                    requestOptions: new QueryRequestOptions { MaxItemCount = 1 }
                );

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync(cancellationToken);
                    var user = response.FirstOrDefault();
                    if (user != null)
                    {
                        return user;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while fetching user by GoogleSub '{GoogleSub}'.",
                    googleSub
                );
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var response = await _container.ReadItemAsync<User>(
                    id: id,
                    partitionKey: new PartitionKey(id),
                    cancellationToken: cancellationToken
                );

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("User with ID '{Id}' not found.", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with ID '{Id}'.", id);
                throw;
            }
        }

        public async Task<User> CreateAsync(
            User user,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var response = await _container.CreateItemAsync(
                    item: user,
                    partitionKey: new PartitionKey(user.Id),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("User {Id} created successfully.", user.Id);
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user with ID '{Id}'.", user.Id);
                throw;
            }
        }

        public async Task<User> UpdateAsync(
            User user,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var response = await _container.ReplaceItemAsync(
                    item: user,
                    id: user.Id,
                    partitionKey: new PartitionKey(user.Id),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("User {Id} updated successfully.", user.Id);
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID '{Id}'.", user.Id);
                throw;
            }
        }
    }
}
