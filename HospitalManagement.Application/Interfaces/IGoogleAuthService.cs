using HospitalManagement.Application.DTO;

namespace HospitalManagement.Application.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfoDto?> ValidateGoogleTokenAsync(
            string idToken,
            CancellationToken cancellationToken = default
        );
    }
}
