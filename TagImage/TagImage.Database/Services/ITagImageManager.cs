using TagImage.Database.Models;

namespace TagImage.Database.Services;

public interface ITagImageManager
{
    Task<IEnumerable<TagEntry>> GetAllTagsAsync();
    Task<IEnumerable<ImageEntry>> GetAllImagesAsync();
    Task<IEnumerable<TagEntry>> GetAllTagsAsync(ImageEntry imageEntry);
    Task<IEnumerable<ImageEntry>> GetAllImagesAsync(TagEntry tagEntry);
    Task TagImageAsync(ImageEntry imageEntry, params IEnumerable<TagEntry> tagEntries);
    Task DetagImageAsync(ImageEntry imageEntry, params IEnumerable<TagEntry> tagEntries);
}