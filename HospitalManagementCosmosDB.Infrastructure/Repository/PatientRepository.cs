using System.Net;
using HospitalManagementCosmosDB.Application.Interfaces;
using HospitalManagementCosmosDB.Domain.Entities;
using HospitalManagementCosmosDB.Infrastructure.Injection;
using Microsoft.Azure.Cosmos;

namespace HospitalManagementCosmosDB.Infrastructure.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly Container _container;

        //public PatientRepository(Container container)
        //{
        //    _container = container;
        //}

        public PatientRepository(CosmosContainerFactory factory)
        {
            _container = factory.GetContainer("Patients");
        }

        // GET ALL
        public async Task<List<Patient>> GetAll()
        {
            var query = _container.GetItemQueryIterator<Patient>(
                new QueryDefinition("SELECT * FROM c")
            );

            var results = new List<Patient>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.Resource);
            }

            return results;
        }

        // GET BY ID
        public async Task<Patient?> GetById(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Patient>(id, new PartitionKey(id));

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        // CREATE
        public async Task<Patient> Create(Patient patient)
        {
            var response = await _container.CreateItemAsync(patient, new PartitionKey(patient.Id));

            return response.Resource;
        }

        // UPDATE (UPSERT)
        public async Task<Patient> UpdateById(Patient patient)
        {
            var response = await _container.ReplaceItemAsync(
                patient,
                patient.Id,
                new PartitionKey(patient.Id)
            );

            return response.Resource;
        }

        // DELETE
        public async Task Delete(string id)
        {
            await _container.DeleteItemAsync<Patient>(id, new PartitionKey(id));
        }
    }
}
