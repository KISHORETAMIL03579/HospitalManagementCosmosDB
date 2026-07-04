using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementCosmosDB.Domain.Entities
{
    public class Patient
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 120)]
        public int Age { get; set; }

        [Required]
        [StringLength(200)]
        public string Disease { get; set; } = string.Empty;
    }
}