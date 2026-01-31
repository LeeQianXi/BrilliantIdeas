using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace DIAbstract;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseBrilliantInitializer()
        {
            collection.AddHostedService<ServiceInitializer>();
            return collection;
        }

        public IServiceCollection AddMultiSingleton<TService,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return collection.AddSingleton<TImplementation>()
                .AddSingleton<TService, TImplementation>(s => s.GetRequiredService<TImplementation>());
        }
    }
}