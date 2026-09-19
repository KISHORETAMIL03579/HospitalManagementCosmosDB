using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HospitalManagement.Application.Common;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HospitalManagement.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            _logger = logger;
            _issuer = configuration["Jwt:Issuer"] ?? "HospitalAPI";
            _audience = configuration["Jwt:Audience"] ?? "HospitalUsers";
            _expiryMinutes = configuration.GetValue<int>("Jwt:ExpiryMinutes", 30);

            var key =
                configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key configuration 'Jwt:Key' is missing."
                );

            if (Encoding.UTF8.GetByteCount(key) < 32)
            {
                throw new InvalidOperationException(
                    "JWT Key must be at least 256 bits (32 bytes) long for HMAC SHA256 security."
                );
            }

            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        public (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user)
        {
            var now = DateTimeOffset.UtcNow;
            var expiresAt = now.AddMinutes(_expiryMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(CustomClaimTypes.GoogleSub, user.GoogleSub ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    now.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64
                ),
            };

            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            return (token, expiresAt);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _securityKey,
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out SecurityToken securityToken
                );
                if (
                    securityToken is not JwtSecurityToken jwtSecurityToken
                    || !string.Equals(
                        jwtSecurityToken.Header.Alg,
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.Ordinal
                    )
                )
                {
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to validate expired JWT.");
                return null;
            }
        }
    }
}
