using HospitalManagement.Application.AutoMapping;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMap>();
            });

            services.AddScoped<Interfaces.IAuthService, Services.AuthService>();

            return services;
        }
    }
}
