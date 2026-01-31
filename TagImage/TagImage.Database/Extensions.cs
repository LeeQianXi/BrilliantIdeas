using DIAbstract;

namespace TagImage.Database;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseTagImageDbCore()
        {
            return collection
                .AddSingleton<ConnectionStorage>()
                .AddSingleton<IAsyncLifecycle, ConnectionStorage>(s => s.GetRequiredService<ConnectionStorage>())
                .AddSingleton<IConnectionStorage, ConnectionStorage>(s => s.GetRequiredService<ConnectionStorage>())
                .AddSingleton<ImageStorage>()
                .AddSingleton<IAsyncLifecycle, ImageStorage>(s => s.GetRequiredService<ImageStorage>())
                .AddSingleton<IImageStorage, ImageStorage>(s => s.GetRequiredService<ImageStorage>())
                .AddSingleton<TagStorage>()
                .AddSingleton<IAsyncLifecycle, TagStorage>(s => s.GetRequiredService<TagStorage>())
                .AddSingleton<ITagStorage, TagStorage>(s => s.GetRequiredService<TagStorage>())
                .AddMultiSingleton<ITagImageManager, TagImageManager>();
        }
    }
}