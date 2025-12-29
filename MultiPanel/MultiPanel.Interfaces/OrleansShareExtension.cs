using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Shared;

namespace MultiPanel.Interfaces;

public static class OrleansShareExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseMultiPanelOrleansServices(IConfiguration configuration)
        {
            return collection
                .UseSharedServices(configuration);
        }
    }
}