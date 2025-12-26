using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Shared;

namespace MultiPanel.Interfaces;

public static class OrleansShareExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseMultiPanelOrleansServices()
        {
            return collection
                .UseSharedServices();
        }
    }
}