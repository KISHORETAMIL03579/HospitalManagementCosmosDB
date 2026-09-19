using System.Security.Claims;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
