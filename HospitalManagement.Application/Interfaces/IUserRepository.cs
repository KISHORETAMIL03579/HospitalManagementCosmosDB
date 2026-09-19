using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByGoogleSubAsync(
            string googleSub,
            CancellationToken cancellationToken = default
        );
        Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
