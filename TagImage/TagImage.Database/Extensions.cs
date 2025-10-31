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