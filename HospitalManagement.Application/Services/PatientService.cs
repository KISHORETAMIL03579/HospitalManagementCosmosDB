using AutoMapper;
using HospitalManagementCosmosDB.Application.DTO;
using HospitalManagementCosmosDB.Application.Interfaces;
using HospitalManagementCosmosDB.Domain.Entities;

namespace HospitalManagementCosmosDB.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repo;
        private readonly IMapper _mapper;

        public PatientService(IPatientRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<PatientDTO>> GetAll()
        {
            var patients = await _repo.GetAll();
            return _mapper.Map<List<PatientDTO>>(patients);
        }

        public async Task<PatientDTO?> GetById(string id)
        {
            var patient = await _repo.GetById(id);
            if (patient == null)
                throw new Exception("Patient Not Found");

            return _mapper.Map<PatientDTO>(patient);
        }

        public async Task<PatientDTO> Create(CreatePatientDTO dto)
        {
            var patient = _mapper.Map<Patient>(dto);

            Console.WriteLine($"Creating patient with Id={patient.Id}");
            var created = await _repo.Create(patient);

            return _mapper.Map<PatientDTO>(created);
        }

        public async Task<PatientDTO> UpdateById(string id, UpdatePatientDTO dto)
        {
            var existing = await _repo.GetById(id);
            if (existing == null)
                throw new Exception("Patient Not Found");
            var patient = _mapper.Map(dto, existing);

            var updated = await _repo.UpdateById(patient);

            return _mapper.Map<PatientDTO>(updated);
        }

        public async Task Delete(string id)
        {
            var existing = await _repo.GetById(id);
            if (existing == null)
                throw new Exception("Patient Not Found");
            await _repo.Delete(id);
        }
    }
}
