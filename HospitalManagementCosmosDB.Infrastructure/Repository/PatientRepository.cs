using System.Net;
using HospitalManagementCosmosDB.Application.Interfaces;
using HospitalManagementCosmosDB.Domain.Entities;
using HospitalManagementCosmosDB.Infrastructure.Injection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace HospitalManagementCosmosDB.Infrastructure.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly Container _container;
        private readonly ILogger<PatientRepository> _logger;

        //public PatientRepository(Container container)
        //{
        //    _container = container;
        //}

        public PatientRepository(CosmosContainerFactory factory, ILogger<PatientRepository> logger)
        {
            _container = factory.GetContainer("Patients");
            _logger = logger;
        }

        // GET ALL
        public async Task<List<Patient>> GetAll(CancellationToken cancellationToken = default)
        {
            var query = _container.GetItemQueryIterator<Patient>(
                queryDefinition: new QueryDefinition("SELECT * FROM c"),
                requestOptions: new QueryRequestOptions { MaxItemCount = 100 }
            );

            var results = new List<Patient>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync(cancellationToken);

                results.AddRange(response.Resource);
            }

            _logger.LogInformation("Retrieved {TotalCount} patients successfully.", results.Count);

            return results;
        }

        // GET BY ID
        public async Task<Patient?> GetById(
            string id,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var response = await _container.ReadItemAsync<Patient>(
                    id: id,
                    partitionKey: new PartitionKey(id),
                    cancellationToken: cancellationToken
                );

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Patient with ID '{Id}' not found.", id);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving patient with ID '{Id}'.", id);

                throw;
            }
        }

        // CREATE
        public async Task<Patient> Create(
            Patient patient,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var response = await _container.CreateItemAsync(
                    item: patient,
                    partitionKey: new PartitionKey(patient.Id),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Patient {Id} created successfully.", patient.Id);

                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while creating patient with ID '{Id}'.",
                    patient.Id
                );

                throw;
            }
        }

        // UPDATE (UPSERT)
        public async Task<Patient> UpdateById(
            Patient patient,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var response = await _container.ReplaceItemAsync(
                    item: patient,
                    id: patient.Id,
                    partitionKey: new PartitionKey(patient.Id),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Patient {Id} updated successfully.", patient.Id);

                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while updating patient with ID '{Id}'.",
                    patient.Id
                );

                throw;
            }
        }

        // DELETE
        public async Task Delete(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                await _container.DeleteItemAsync<Patient>(
                    id: id,
                    partitionKey: new PartitionKey(id),
                    cancellationToken: cancellationToken
                );

                _logger.LogInformation("Patient {Id} deleted successfully.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting patient with ID '{Id}'.", id);

                throw;
            }
        }
    }
}
