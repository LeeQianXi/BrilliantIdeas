using System.Drawing;
using NetUtility;
using SQLite;

namespace ToDoListDb.Abstract.Model;

[Table(nameof(BackGroup))]
public class BackGroup
{
    [PrimaryKey, AutoIncrement, Column(nameof(GroupId))]
    public int GroupId { get; set; }

    [Column(nameof(GroupName))] public string GroupName { get; set; } = "Default";
    [Column(nameof(ColorArgb))] public int ColorArgb { get; set; } = Color.White.ToArgb();

    [Ignore]
    public Color GroupColor
    {
        get => Color.FromArgb(ColorArgb);
        set => ColorArgb = value.ToArgb();
    }

    public static BackGroup CreateNew(string groupName, string groupColor = "#FFFFFFFF")
    {
        return new BackGroup
        {
            GroupName = groupName,
            ColorArgb = groupColor.StringToArgbInt()
        };
    }

    public static BackGroup CreateNew(string groupName, int groupColor)
    {
        return new BackGroup
        {
            GroupName = groupName,
            ColorArgb = groupColor
        };
    }

    public static readonly BackGroup Default = new();
}