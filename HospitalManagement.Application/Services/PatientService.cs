using AutoMapper;
using HospitalManagementCosmosDB.Application.DTO;
using HospitalManagementCosmosDB.Application.Interfaces;
using HospitalManagementCosmosDB.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HospitalManagementCosmosDB.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<PatientService> _logger;

        public PatientService(
            IPatientRepository repo,
            IMapper mapper,
            ILogger<PatientService> logger
        )
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        // GET ALL
        public async Task<List<PatientDTO>> GetAll(CancellationToken cancellationToken = default)
        {
            var patients = await _repo.GetAll(cancellationToken);

            _logger.LogInformation("Retrieved {Count} patients.", patients.Count);

            return _mapper.Map<List<PatientDTO>>(patients);
        }

        // GET BY ID
        public async Task<PatientDTO?> GetById(
            string id,
            CancellationToken cancellationToken = default
        )
        {
            var patient = await _repo.GetById(id, cancellationToken);

            if (patient == null)
            {
                throw new KeyNotFoundException($"Patient with ID '{id}' was not found.");
            }

            _logger.LogInformation("Retrieved patient with ID {Id}.", id);

            return _mapper.Map<PatientDTO>(patient);
        }

        // CREATE
        public async Task<PatientDTO> Create(
            CreatePatientDTO dto,
            CancellationToken cancellationToken = default
        )
        {
            var patient = _mapper.Map<Patient>(dto);

            patient.Id = Guid.NewGuid().ToString("N");

            _logger.LogInformation("Creating patient with ID {Id}.", patient.Id);

            var created = await _repo.Create(patient, cancellationToken);

            _logger.LogInformation("Patient with ID {Id} created successfully.", created.Id);

            return _mapper.Map<PatientDTO>(created);
        }

        // UPDATE
        public async Task<PatientDTO> UpdateById(
            string id,
            UpdatePatientDTO dto,
            CancellationToken cancellationToken = default
        )
        {
            var existing = await _repo.GetById(id, cancellationToken);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Patient with ID '{id}' was not found.");
            }

            _mapper.Map(dto, existing);

            var updated = await _repo.UpdateById(existing, cancellationToken);

            _logger.LogInformation("Patient with ID {Id} updated successfully.", id);

            return _mapper.Map<PatientDTO>(updated);
        }

        // DELETE
        public async Task Delete(string id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetById(id, cancellationToken);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Patient with ID '{id}' was not found.");
            }

            await _repo.Delete(id, cancellationToken);

            _logger.LogInformation("Patient with ID {Id} deleted successfully.", id);
        }
    }
}
