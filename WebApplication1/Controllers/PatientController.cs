using HospitalManagementCosmosDB.Application.DTO;
using HospitalManagementCosmosDB.Application.Helpers;
using HospitalManagementCosmosDB.Application.Interfaces;
using HospitalManagementCosmosDB.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HospitalManagementCosmosDB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _service;
        private readonly IIdempotencyRepository _idempotencyRepository;
        private readonly ILogger<PatientController> _logger;

        public PatientController(
            IPatientService service,
            IIdempotencyRepository idempotencyRepository,
            ILogger<PatientController> logger
        )
        {
            _service = service;
            _idempotencyRepository = idempotencyRepository;
            _logger = logger;
        }

        #region Get All

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var patients = await _service.GetAll(cancellationToken);

            return Ok(patients);
        }

        #endregion

        #region Get By Id

        [HttpGet]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var patient = await _service.GetById(id, cancellationToken);

            return Ok(patient);
        }

        #endregion

        #region Create (Idempotent)

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreatePatientDTO dto,
            CancellationToken cancellationToken
        )
        {
            if (!Request.Headers.TryGetValue("Idempotency-Key", out var key))
            {
                return BadRequest("Idempotency-Key header is required.");
            }

            _logger.LogInformation(
                "Received create patient request with Idempotency-Key: {Key}",
                key.ToString()
            );

            var requestHash = RequestHashHelper.ComputeHash(dto);

            // Check whether this key already exists
            var existing = await _idempotencyRepository.GetAsync(key!, cancellationToken);

            // Same key but different request
            if (existing != null && existing.RequestHash != requestHash)
            {
                _logger.LogWarning(
                    "Idempotency key {Key} reused with different request body.",
                    key.ToString()
                );

                return Conflict("Idempotency-Key reuse with different request body.");
            }

            // Same key + same request
            if (existing != null)
            {
                _logger.LogInformation(
                    "Returning cached response for Idempotency-Key {Key}.",
                    key.ToString()
                );

                return Ok(JsonConvert.DeserializeObject(existing.ResponseJson));
            }

            // Create patient
            var result = await _service.Create(dto, cancellationToken);

            // Save idempotency record
            await _idempotencyRepository.SaveAsync(
                new Idempotency
                {
                    Id = key!,
                    RequestHash = requestHash,
                    ResponseJson = JsonConvert.SerializeObject(result),
                },
                cancellationToken
            );

            _logger.LogInformation(
                "Patient created successfully with Idempotency-Key {Key}.",
                key.ToString()
            );

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        #endregion

        #region Update

        [HttpPut]
        public async Task<IActionResult> UpdateById(
            string id,
            [FromBody] UpdatePatientDTO dto,
            CancellationToken cancellationToken
        )
        {
            var updatedPatient = await _service.UpdateById(id, dto, cancellationToken);

            return Ok(updatedPatient);
        }

        #endregion

        #region Delete

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(string id, CancellationToken cancellationToken)
        {
            await _service.Delete(id, cancellationToken);

            return NoContent();
        }

        #endregion
    }
}
