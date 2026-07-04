using Newtonsoft.Json;

namespace HospitalManagementCosmosDB.Domain.Entities
{
    public class Patient
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N"); // remove hyphens for Cosmos DB id

        //public string id { get; set; } = string.Empty; //= Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Disease { get; set; } = string.Empty;
    }
}
