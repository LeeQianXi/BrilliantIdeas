using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Shared.Services;

namespace MultiPanel.Shared;

public static class SharedExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseSharedServices(IConfiguration configuration)
        {
            return collection
                .Configure<JwtOption>(configuration.GetSection(nameof(JwtOption)))
                .AddSingleton<ITokenGenerator, JwtTokenGenerator>()
                .AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        }
    }
}