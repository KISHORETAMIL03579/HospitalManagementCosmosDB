namespace HospitalManagement.Application.DTO
{
    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }
}
