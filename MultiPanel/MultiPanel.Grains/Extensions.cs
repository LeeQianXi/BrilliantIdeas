using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Abstractions.IRepository;
using MultiPanel.Grains.Persistence;
using MultiPanel.Shared;

namespace MultiPanel.Grains;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseSileServices(IConfiguration configuration)
        {
            return collection
                .UseSharedServices(configuration)
                .AddSingleton<IAccountRepository, AccountRepository>();
        }
    }
}