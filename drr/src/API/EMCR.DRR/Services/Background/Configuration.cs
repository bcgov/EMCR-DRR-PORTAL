using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EMCR.DRR.API.Services.Background
{
    public static class Configuration
    {
        public static IServiceCollection AddBackgroundTasks(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddTransient<CASBackgroundTask>();
            services.AddHostedService<BackgroundTask<CASBackgroundTask>>();

            return services;
        }
    }
}
