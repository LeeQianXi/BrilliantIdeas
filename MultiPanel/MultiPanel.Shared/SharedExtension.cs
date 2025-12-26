using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Shared.Services;

namespace MultiPanel.Shared;

public static class SharedExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseSharedServices()
        {
            return collection
                .AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        }
    }
}