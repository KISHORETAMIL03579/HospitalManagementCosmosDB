using HospitalManagementCosmosDB.Application.AutoMapping;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalManagementCosmosDB.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMap>();
            });

            return services;
        }
    }
}
