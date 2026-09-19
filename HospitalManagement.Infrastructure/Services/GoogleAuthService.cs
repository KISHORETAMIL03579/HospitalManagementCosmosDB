using Google.Apis.Auth;
using HospitalManagement.Application.DTO;
using HospitalManagement.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GoogleUserInfoDto?> ValidateGoogleTokenAsync(
            string idToken,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var clientId = _configuration["Google:ClientId"];
                if (
                    string.IsNullOrWhiteSpace(clientId)
                    || clientId.Contains("YOUR_GOOGLE_CLIENT_ID")
                )
                {
                    _logger.LogCritical(
                        "Google ClientId configuration is missing or unconfigured."
                    );
                    throw new InvalidOperationException(
                        "Google ClientId configuration 'Google:ClientId' is missing."
                    );
                }

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId },
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                if (payload == null)
                {
                    return null;
                }

                return new GoogleUserInfoDto
                {
                    GoogleSub = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                };
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token provided.");
                return null;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating Google ID token.");
                return null;
            }
        }
    }
}
