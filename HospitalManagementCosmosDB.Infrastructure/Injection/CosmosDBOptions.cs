using System.ComponentModel.DataAnnotations;

namespace HospitalManagementCosmosDB.Infrastructure.Injection
{
    public class CosmosDbOptions
    {
        [Required]
        public string AccountEndpoint { get; set; } = string.Empty;

        [Required]
        public string AccountKey { get; set; } = string.Empty;

        [Required]
        public string DatabaseId { get; set; } = string.Empty;

        [Required]
        public string ConnectionMode { get; set; } = string.Empty;

        [Required]
        public RetryOptions RetryOptions { get; set; } = new();

        [Required]
        public List<ContainerOptions> Containers { get; set; } = new();
    }

    public class ContainerOptions
    {
        [Required]
        public string ContainerId { get; set; } = string.Empty;

        [Required]
        public string PartitionKeyPath { get; set; } = string.Empty;

        [Required]
        public string Throughput { get; set; } = string.Empty;
    }

    public class RetryOptions
    {
        [Range(1, 10)]
        public int MaxRetryAttempts { get; set; }

        [Range(1, 60)]
        public int MaxRetryWaitTimeSeconds { get; set; }
    }
}
