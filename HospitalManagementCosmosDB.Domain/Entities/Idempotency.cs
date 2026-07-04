using Newtonsoft.Json;

namespace HospitalManagementCosmosDB.Domain.Entities
{
    public class Idempotency
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        public string RequestHash { get; set; } = null!;

        public string ResponseJson { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
