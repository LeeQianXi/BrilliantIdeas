namespace DataBaseAbstract.Helpers;

public static class PathHelper
{
    [field: AllowNull]
    public static string LocalFolderPath
    {
        get
        {
            field ??= Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Path.GetFileNameWithoutExtension(Environment.ProcessPath ?? nameof(DataBaseAbstract))
            );
            if (!Path.Exists(field))
                Directory.CreateDirectory(field);
            return field;
        }
    }

    public static string GetLocalFilePath(string filename)
    {
        return Path.Combine(LocalFolderPath, filename);
    }
}