using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HospitalManagement.Domain.Entities
{
    public class Idempotency
    {
        [JsonProperty("id")]
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string RequestHash { get; set; } = string.Empty;

        [Required]
        public string ResponseJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
