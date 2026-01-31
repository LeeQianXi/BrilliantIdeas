namespace TagImage.Database.Manager;

public sealed class TagImageManager(
    IConnectionStorage connectionStorage,
    IImageStorage imageStorage,
    ITagStorage tagStorage
) : ITagImageManager
{
    public async Task<IEnumerable<TagEntry>> GetAllTagsAsync()
    {
        IEnumerable<TagEntry> tags = [];
        await foreach (var item in tagStorage.SelectDatasAsync()) tags = tags.Concat(item);

        return tags;
    }

    public async Task<IEnumerable<ImageEntry>> GetAllImagesAsync()
    {
        IEnumerable<ImageEntry> tags = [];
        await foreach (var item in imageStorage.SelectDatasAsync()) tags = tags.Concat(item);

        return tags;
    }

    public async Task<IEnumerable<TagEntry>> GetAllTagsAsync(ImageEntry imageEntry)
    {
        Validate(imageEntry);
        IEnumerable<ConnectionEntry> tags = [];
        var pk = imageEntry.PrimaryKey;
        await foreach (var item in connectionStorage.SelectDatasAsync(ce => ce.ImgId == pk))
            tags = tags.Concat(item);

        IEnumerable<TagEntry> tes = [];
        await tagStorage.BeginTransactionAsync(con =>
        {
            con.BeginTransaction();
            tes = tags.AsParallel().Select(ce => con.Find<TagEntry>(ce.TagId)).Where(ce => ce is not null)
                .AsEnumerable();
            con.Commit();
        });

        return tes;
    }

    public async Task<IEnumerable<ImageEntry>> GetAllImagesAsync(TagEntry tagEntry)
    {
        Validate(tagEntry);
        IEnumerable<ConnectionEntry> tags = [];
        var pk = tagEntry.PrimaryKey;
        await foreach (var item in connectionStorage.SelectDatasAsync(ce => ce.TagId == pk)) tags = tags.Concat(item);

        IEnumerable<ImageEntry> ies = [];
        await tagStorage.BeginTransactionAsync(con =>
        {
            con.BeginTransaction();
            ies = tags.AsParallel().Select(ce => con.Find<ImageEntry>(ce.ImgId)).Where(ie => ie is not null)
                .AsEnumerable();
            con.Commit();
        });

        return ies;
    }

    public async Task TagImageAsync(ImageEntry imageEntry, params IEnumerable<TagEntry> tagEntries)
    {
        Validate(imageEntry);
    }

    public async Task DetagImageAsync(ImageEntry imageEntry, params IEnumerable<TagEntry> tagEntries)
    {
        Validate(imageEntry);
    }

    private static void Validate(ImageEntry imageEntry)
    {
        if (!imageEntry.IsEnabled)
            throw new ArgumentException("this ImageEntry is not enabled");
    }

    private static void Validate(TagEntry tagEntry)
    {
        if (!tagEntry.IsEnabled)
            throw new ArgumentException("this TagEntry is not enabled");
    }

    private static void Validate(ConnectionEntry connectionEntry)
    {
        if (!connectionEntry.IsActive)
            throw new ArgumentException("this ConnectionEntry is not active");
    }
}