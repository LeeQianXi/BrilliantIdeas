using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Services;

namespace MultiPanel.Silo.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationServices()
        {
            return services;
        }

        public IServiceCollection AddInfrastructureServices(IConfiguration configuration)
        {
            return services;
        }

        public IServiceCollection AddGrainServices()
        {
            // 注册需要注入到Grains中的服务
            services.AddSingleton<IGrainService, GrainService>();

            return services;
        }
    }
}