using DataBaseAbstract.Services;
using SQLite;

namespace TagImage.Database.Models;

[Table(nameof(ConnectionEntry))]
public class ConnectionEntry : IModelBasic
{
    public int PrimaryKey { get; set; }
    public int ImgId { get; set; }
    public int TagId { get; set; }
    public bool IsActive { get; set; }

    public static ConnectionEntry Create(ImageEntry img, TagEntry tag)
    {
        return new ConnectionEntry
        {
            ImgId = img.PrimaryKey,
            TagId = tag.PrimaryKey,
            IsActive = img.IsEnabled && tag.IsEnabled
        };
    }
}