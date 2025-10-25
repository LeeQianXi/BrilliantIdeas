using DataBaseAbstract.Services;
using SQLite;

namespace TagImage.Database.Models;

[Table(nameof(TagEntry))]
public class TagEntry : IModelBasic
{
    [AutoIncrement, PrimaryKey] public int PrimaryKey { get; set; }
    public string TagName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = false;

    public static TagEntry Create(string tagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagName);
        return new TagEntry
        {
            TagName = tagName,
        };
    }
}