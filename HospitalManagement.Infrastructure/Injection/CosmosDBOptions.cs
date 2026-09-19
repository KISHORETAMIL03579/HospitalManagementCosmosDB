using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Infrastructure.Injection
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

        public int? Throughput { get; set; } = 400;
    }

    public class RetryOptions
    {
        [Range(1, 10)]
        public int MaxRetryAttempts { get; set; }

        [Range(1, 60)]
        public int MaxRetryWaitTimeSeconds { get; set; }
    }
}
