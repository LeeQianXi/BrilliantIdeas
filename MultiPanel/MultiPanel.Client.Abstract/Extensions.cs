using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Client.Abstract.Services;

namespace MultiPanel.Client.Abstract;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseClientServices()
        {
            return collection
                .AddSingleton<IWritableConfigureFactory, WritableConfigureFactory>();
        }
    }
}