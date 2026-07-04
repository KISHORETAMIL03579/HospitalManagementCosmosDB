using HospitalManagementCosmosDB.Domain.Entities;

namespace HospitalManagementCosmosDB.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAll(CancellationToken cancellationToken = default);
        Task<Patient?> GetById(string id, CancellationToken cancellationToken = default);
        Task<Patient> Create(Patient patient, CancellationToken cancellationToken = default);
        Task<Patient> UpdateById(Patient patient, CancellationToken cancellationToken = default);
        Task Delete(string id, CancellationToken cancellationToken = default);
    }
}
