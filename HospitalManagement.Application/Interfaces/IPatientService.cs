using HospitalManagementCosmosDB.Application.DTO;

namespace HospitalManagementCosmosDB.Application.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientDTO>> GetAll(CancellationToken cancellationToken = default);
        Task<PatientDTO?> GetById(string id, CancellationToken cancellationToken = default);
        Task<PatientDTO> Create(
            CreatePatientDTO dto,
            CancellationToken cancellationToken = default
        );
        Task<PatientDTO> UpdateById(
            string id,
            UpdatePatientDTO dto,
            CancellationToken cancellationToken = default
        );
        Task Delete(string id, CancellationToken cancellationToken = default);
    }
}
