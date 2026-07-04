using HospitalManagementCosmosDB.Domain.Entities;

namespace HospitalManagementCosmosDB.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAll();
        Task<Patient?> GetById(string id);
        Task<Patient> Create(Patient patient);
        Task<Patient> UpdateById(Patient patient);
        Task Delete(string id);
    }
}
