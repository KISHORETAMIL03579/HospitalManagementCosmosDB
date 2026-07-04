using HospitalManagementCosmosDB.Application.Interfaces;
using HospitalManagementCosmosDB.Application.Services;
using HospitalManagementCosmosDB.Infrastructure.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HospitalManagementCosmosDB.Infrastructure.Injection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureCosmos(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            //services.Configure<CosmosDbOptions>(configuration.GetSection("CosmosDb"));

            services
                .AddOptions<CosmosDbOptions>()
                .Bind(configuration.GetSection("CosmosDb"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            //services.AddSingleton(sp =>
            //{
            //    var opt = sp.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            //    return new CosmosClient(opt.AccountEndpoint, opt.AccountKey);
            //});

            //services.AddSingleton(sp =>
            //{
            //    var opt = sp.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

            //    return new CosmosClient(
            //        opt.AccountEndpoint,
            //        opt.AccountKey,
            //        new CosmosClientOptions
            //        {
            //            ConnectionMode = ConnectionMode.Gateway,
            //            HttpClientFactory = () =>
            //            {
            //                var handler = new HttpClientHandler
            //                {
            //                    ServerCertificateCustomValidationCallback =
            //                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            //                };
            //                return new HttpClient(handler);
            //            },
            //        }
            //    );
            //});

            //services.AddSingleton(sp =>
            //{
            //    var opt = sp.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            //    var client = sp.GetRequiredService<CosmosClient>();

            //    return client.GetContainer(opt.DatabaseId, opt.ContainerId);
            //});

            services.AddSingleton(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
                if (
                    !Enum.TryParse<ConnectionMode>(
                        opt.ConnectionMode,
                        ignoreCase: true,
                        out var connectionMode
                    )
                )
                {
                    throw new InvalidOperationException(
                        $"Invalid ConnectionMode: {opt.ConnectionMode}"
                    );
                }

                var clientOptions = new CosmosClientOptions
                {
                    ConnectionMode = connectionMode,

                    MaxRetryAttemptsOnRateLimitedRequests = opt.RetryOptions.MaxRetryAttempts,

                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(
                        opt.RetryOptions.MaxRetryWaitTimeSeconds
                    ),

                    HttpClientFactory = () =>
                    {
                        var handler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                        };

                        return new HttpClient(handler);
                    },
                };

                return new CosmosClient(opt.AccountEndpoint, opt.AccountKey, clientOptions);
            });

            services.AddSingleton<CosmosContainerFactory>();

            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddSingleton<IIdempotencyRepository, IdempotencyRepository>();

            return services;
        }
    }
}
