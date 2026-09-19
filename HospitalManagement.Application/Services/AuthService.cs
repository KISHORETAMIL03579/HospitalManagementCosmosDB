using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HospitalManagement.Application.Common;
using HospitalManagement.Application.DTO;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IGoogleAuthService googleAuthService,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<AuthService> logger
        )
        {
            _userRepository = userRepository;
            _googleAuthService = googleAuthService;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> ContinueWithGoogleAsync(
            string idToken,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                return AuthResponse.Fail(
                    "Google ID token is required.",
                    AuthErrorCodes.InvalidInput
                );
            }

            var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(
                idToken,
                cancellationToken
            );
            if (googleUser == null)
            {
                _logger.LogWarning("Google token validation failed.");
                return AuthResponse.Fail(
                    "Invalid Google ID token.",
                    AuthErrorCodes.InvalidGoogleToken
                );
            }

            return await AuthenticateGoogleUserAsync(googleUser, cancellationToken);
        }

        public async Task<AuthResponse> RefreshTokenAsync(
            string accessToken,
            string refreshToken,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return AuthResponse.Fail(
                    "Access token and refresh token are required.",
                    AuthErrorCodes.InvalidInput
                );
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return AuthResponse.Fail(
                    "Invalid access token.",
                    AuthErrorCodes.InvalidAccessToken
                );
            }

            var userId =
                GetClaimValue(principal, ClaimTypes.NameIdentifier)
                ?? GetClaimValue(principal, "sub");

            if (string.IsNullOrEmpty(userId))
            {
                return AuthResponse.Fail(
                    "Invalid access token claims.",
                    AuthErrorCodes.InvalidClaims
                );
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (
                user == null
                || string.IsNullOrEmpty(user.RefreshToken)
                || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow
            )
            {
                _logger.LogWarning(
                    "Invalid or expired refresh token attempt for user {UserId}.",
                    userId
                );
                return AuthResponse.Fail(
                    "Invalid or expired refresh token. Please log in again.",
                    AuthErrorCodes.InvalidRefreshToken
                );
            }

            byte[] storedHashBytes;
            try
            {
                storedHashBytes = Convert.FromBase64String(user.RefreshToken);
            }
            catch (FormatException)
            {
                _logger.LogWarning(
                    "Corrupted refresh token in database for user {UserId}.",
                    user.Id
                );
                return AuthResponse.Fail(
                    "Invalid refresh token.",
                    AuthErrorCodes.InvalidRefreshToken
                );
            }

            var incomingHashBytes = HashTokenBytes(refreshToken);

            if (!CryptographicOperations.FixedTimeEquals(storedHashBytes, incomingHashBytes))
            {
                _logger.LogWarning(
                    "Constant-time refresh token mismatch for user {UserId}.",
                    userId
                );
                return AuthResponse.Fail(
                    "Invalid or expired refresh token. Please log in again.",
                    AuthErrorCodes.InvalidRefreshToken
                );
            }

            var (newAccessToken, expiresAt) = _tokenService.GenerateAccessToken(user);
            var newRawRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = Convert.ToBase64String(HashTokenBytes(newRawRefreshToken));
            user.RefreshTokenExpiryTime = GetRefreshTokenExpiryTime();
            await _userRepository.UpdateAsync(user, cancellationToken);

            return AuthResponse.Ok(
                newAccessToken,
                newRawRefreshToken,
                expiresAt,
                MapToUserDto(user),
                "Token refreshed successfully."
            );
        }

        public async Task<AuthResponse> LogoutAsync(
            string userId,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return AuthResponse.Fail("User ID is required.", AuthErrorCodes.InvalidInput);
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userRepository.UpdateAsync(user, cancellationToken);
                _logger.LogInformation("User {UserId} logged out successfully.", userId);
            }

            return new AuthResponse { Success = true, Message = "Logged out successfully." };
        }

        private async Task<AuthResponse> AuthenticateGoogleUserAsync(
            GoogleUserInfoDto googleUser,
            CancellationToken cancellationToken
        )
        {
            var user = await _userRepository.GetByGoogleSubAsync(
                googleUser.GoogleSub,
                cancellationToken
            );
            bool isNewUser = false;

            if (user == null)
            {
                _logger.LogInformation(
                    "No user found with GoogleSub {GoogleSub}. Auto-provisioning new account.",
                    googleUser.GoogleSub
                );

                user = User.CreateFromGoogle(
                    googleUser.GoogleSub,
                    googleUser.Email,
                    googleUser.Name
                );
                await _userRepository.CreateAsync(user, cancellationToken);
                isNewUser = true;
            }
            else
            {
                // Synchronize profile information if updated in Google
                if (user.Name != googleUser.Name || user.Email != googleUser.Email)
                {
                    user.Name = googleUser.Name;
                    user.Email = googleUser.Email;
                }
            }

            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user);
            var rawRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = Convert.ToBase64String(HashTokenBytes(rawRefreshToken));
            user.RefreshTokenExpiryTime = GetRefreshTokenExpiryTime();
            await _userRepository.UpdateAsync(user, cancellationToken);

            var userDto = MapToUserDto(user);
            string message = isNewUser
                ? "Account created and authenticated successfully."
                : "Logged in successfully.";

            return AuthResponse.Ok(accessToken, rawRefreshToken, expiresAt, userDto, message);
        }

        private DateTimeOffset GetRefreshTokenExpiryTime()
        {
            var refreshDaysStr = _configuration["Jwt:RefreshTokenDays"] ?? "7";
            var refreshDays = double.TryParse(refreshDaysStr, out var days) ? days : 7;
            return DateTimeOffset.UtcNow.AddDays(refreshDays);
        }

        private static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
        {
            return principal?.FindFirst(claimType)?.Value;
        }

        private static byte[] HashTokenBytes(string token)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
            };
        }
    }
}
