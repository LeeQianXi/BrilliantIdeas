namespace ToDoList.DataBase.Models;

[Table(nameof(BackGroup))]
public class BackGroup : IModelBasic
{
    public static readonly BackGroup Default = new();

    [Column(nameof(GroupName))] public string GroupName { get; set; } = "Default";
    [Column(nameof(ColorArgb))] public int ColorArgb { get; set; } = Color.White.ToArgb();

    [Ignore]
    public Color GroupColor
    {
        get => Color.FromArgb(ColorArgb);
        set => ColorArgb = value.ToArgb();
    }

    [Column("GroupId")] public int PrimaryKey { get; set; }

    internal static BackGroup CreateNew(string groupName, string groupColor = "#FFFFFFFF")
    {
        return new BackGroup
        {
            GroupName = groupName,
            ColorArgb = groupColor.StringToColor().ToArgb()
        };
    }

    internal static BackGroup CreateNew(string groupName, int groupColor)
    {
        return new BackGroup
        {
            GroupName = groupName,
            ColorArgb = groupColor
        };
    }
}