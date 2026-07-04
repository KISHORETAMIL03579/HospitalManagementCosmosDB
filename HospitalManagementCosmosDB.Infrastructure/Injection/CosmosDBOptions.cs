namespace HospitalManagementCosmosDB.Infrastructure.Injection
{
    public class CosmosDbOptions
    {
        public string AccountEndpoint { get; set; } = null!;
        public string AccountKey { get; set; } = null!;
        public string DatabaseId { get; set; } = null!;
        public List<CosmosContainerOptions> Containers { get; set; } = new();
    }

    public class CosmosContainerOptions
    {
        public string ContainerId { get; set; } = null!;
        public string PartitionKeyPath { get; set; } = "/id";
    }
}
