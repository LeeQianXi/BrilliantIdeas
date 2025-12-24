using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Interfaces.Services;
using MultiPanel.Shared;

namespace MultiPanel.Interfaces;

public static class OrleansShareExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseMultiPanelOrleansServices()
        {
            return collection
                .UseSharedServices()
                .AddScoped<IAuthenticationService, AuthenticationService>();
        }
    }
}