using Microsoft.Extensions.DependencyInjection;
using TagImage.Database.Services;
using TagImage.Database.Instances;
using TagImage.Database.Manager;

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