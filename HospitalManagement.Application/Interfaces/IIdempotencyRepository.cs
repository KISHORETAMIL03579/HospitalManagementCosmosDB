using HospitalManagementCosmosDB.Domain.Entities;

namespace HospitalManagementCosmosDB.Application.Interfaces
{
    public interface IIdempotencyRepository
    {
        Task<Idempotency?> GetAsync(string key, CancellationToken cancellationToken = default);

        Task SaveAsync(Idempotency record, CancellationToken cancellationToken = default);
    }
}
