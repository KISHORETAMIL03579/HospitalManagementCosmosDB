namespace HospitalManagement.Application.DTO
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset? ExpiresAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public UserDto? User { get; set; }

        public static AuthResponse Ok(
            string token,
            string refreshToken,
            DateTimeOffset expiresAt,
            UserDto user,
            string message = "Authentication successful."
        )
        {
            return new AuthResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                Message = message,
                User = user,
            };
        }

        public static AuthResponse Fail(string message, string? errorCode = null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
            };
        }
    }
}
