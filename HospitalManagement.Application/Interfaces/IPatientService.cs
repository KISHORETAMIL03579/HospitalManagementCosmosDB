using HospitalManagementCosmosDB.Application.DTO;

namespace HospitalManagementCosmosDB.Application.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientDTO>> GetAll();
        Task<PatientDTO?> GetById(string id);
        Task<PatientDTO> Create(CreatePatientDTO dto);
        Task<PatientDTO> UpdateById(string id, UpdatePatientDTO dto);
        Task Delete(string id);
    }
}
