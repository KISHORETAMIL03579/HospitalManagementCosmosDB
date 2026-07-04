using HospitalManagementCosmosDB.Application.DTO;
using HospitalManagementCosmosDB.Application.Helpers;
using HospitalManagementCosmosDB.Application.Interfaces;
using HospitalManagementCosmosDB.Domain.Entities;
using HospitalManagementCosmosDB.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HospitalManagementCosmosDB.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _service;

        public PatientController(IPatientService service)
        {
            _service = service;
        }

        #region Get All Method

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _service.GetAll();
            return Ok(patients);
        }

        #endregion

        #region Get By Id Method

        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var patient = await _service.GetById(id);
            return Ok(patient);
        }

        #endregion

        #region Previous Create Method

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreatePatientDTO dto)
        //{
        //    var createdPatient = await _service.Create(dto);
        //    return CreatedAtAction(nameof(GetById), createdPatient);
        //}

        #endregion

        #region Idempotent Create Method

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreatePatientDTO dto,
            [FromServices] IdempotencyRepository repo
        )
        {
            if (!Request.Headers.TryGetValue("Idempotency-Key", out var key))
            {
                return BadRequest("Idempotency-Key header is required");
            }

            var requestHash = RequestHashHelper.ComputeHash(dto);

            // 🔹 Read existing idempotency record
            var existing = await repo.GetAsync(key!);

            // ✅ THIS IS WHERE YOUR CHECK GOES
            if (existing != null && existing.RequestHash != requestHash)
            {
                return Conflict("Idempotency-Key reuse with different request body");
            }

            // 🔹 If same key + same body → return cached response
            if (existing != null)
            {
                return Ok(JsonConvert.DeserializeObject(existing.ResponseJson));
            }

            // 🔹 Create new resource
            var result = await _service.Create(dto);

            // 🔹 Save idempotency record
            await repo.SaveAsync(
                new Idempotency
                {
                    Id = key!,
                    RequestHash = requestHash,
                    ResponseJson = JsonConvert.SerializeObject(result),
                }
            );

            return CreatedAtAction(nameof(GetById), result);
        }

        #endregion

        #region Update Method

        [HttpPut]
        public async Task<IActionResult> UpdateById(string id, [FromBody] UpdatePatientDTO dto)
        {
            var updatedPatient = await _service.UpdateById(id, dto);
            return Ok("Patient Details is Updated");
        }

        #endregion

        #region Delete Method

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(string id)
        {
            await _service.Delete(id);
            return Ok("Patient Details is Deleted");
        }

        #endregion
    }
}
