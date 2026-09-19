using System.Security.Claims;
using HospitalManagement.Application.Common;
using HospitalManagement.Application.DTO;
using HospitalManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Unified "Continue with Google" direct ID token exchange endpoint.
        /// Validates Google ID Token, authenticates/provisions user, and returns JWT Access Token + Refresh Token.
        /// </summary>
        [HttpPost("google")]
        public async Task<IActionResult> ContinueWithGoogle(
            [FromBody] GoogleAuthRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var result = await _authService.ContinueWithGoogleAsync(dto.IdToken, cancellationToken);
            if (!result.Success)
            {
                if (result.ErrorCode == AuthErrorCodes.InvalidGoogleToken)
                {
                    return Unauthorized(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Refreshes Access Token using a valid Refresh Token.
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var result = await _authService.RefreshTokenAsync(
                dto.AccessToken,
                dto.RefreshToken,
                cancellationToken
            );
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Revokes Refresh Token and logs out user.
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(
                    AuthResponse.Fail(
                        "User ID not found in token claims.",
                        AuthErrorCodes.Unauthorized
                    )
                );
            }

            var result = await _authService.LogoutAsync(userId, cancellationToken);
            return Ok(result);
        }
    }
}
