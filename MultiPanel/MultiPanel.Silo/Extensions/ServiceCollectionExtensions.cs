using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Grains;

namespace MultiPanel.Silo.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseSiloExtension(IConfiguration configuration)
        {
            return collection
                .UseSileServices(configuration);
        }
    }
}