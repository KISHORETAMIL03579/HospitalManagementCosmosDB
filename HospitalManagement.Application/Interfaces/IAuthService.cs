using HospitalManagement.Application.DTO;

namespace HospitalManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> ContinueWithGoogleAsync(
            string idToken,
            CancellationToken cancellationToken = default
        );
        Task<AuthResponse> RefreshTokenAsync(
            string accessToken,
            string refreshToken,
            CancellationToken cancellationToken = default
        );
        Task<AuthResponse> LogoutAsync(
            string userId,
            CancellationToken cancellationToken = default
        );
    }
}
