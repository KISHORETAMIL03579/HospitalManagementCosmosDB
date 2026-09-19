using HospitalManagement.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HospitalManagement.Domain.Entities
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("googleSub")]
        public string GoogleSub { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("role")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserRole Role { get; set; } = UserRole.User;

        [JsonProperty("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonProperty("refreshTokenExpiryTime")]
        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Domain Factory method for creating a user authenticated via Google.
        /// </summary>
        public static User CreateFromGoogle(string googleSub, string email, string name)
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                GoogleSub = googleSub,
                Email = email,
                Name = name,
                Role = UserRole.User,
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }
    }
}
