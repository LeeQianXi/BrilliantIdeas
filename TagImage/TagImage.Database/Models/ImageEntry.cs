namespace TagImage.Database.Models;

[Table(nameof(ImageEntry))]
public class ImageEntry : IModelBasic
{
    public string ImgPath { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;

    [AutoIncrement]
    [PrimaryKey]
    [Column("Id")]
    public int PrimaryKey { get; set; }

    public static bool TryCreate(string path, out ImageEntry? entry)
    {
        entry = null;
        if (!File.Exists(path)) return false;

        entry = new ImageEntry
        {
            ImgPath = path,
            Name = Path.GetFileNameWithoutExtension(path)
        };
        return true;
    }
}