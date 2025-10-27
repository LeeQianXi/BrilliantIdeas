using Microsoft.Extensions.DependencyInjection;
using TagImage.Database.Instances;
using TagImage.Database.Manager;
using TagImage.Database.Services;

namespace TagImage.Database;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseTagImageDbCore()
        {
            return collection
                .AddSingleton<ITagImageManager, TagImageManager>();
        }
    }
}