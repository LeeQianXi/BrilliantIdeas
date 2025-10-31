using DataBaseAbstract.Services;

namespace ToDoList.DataBase.Models;

[Table(nameof(BackLog))]
public class BackLog : IModelBasic
{
    public static readonly BackLog Empty = new() { Title = "Empty", PrimaryKey = -1 };

    [Column("Title")] public string Title { get; set; } = string.Empty;
    [Column("Description")] public string Description { get; set; } = string.Empty;
    [Column("TaskStatus")] public TaskStatus Status { get; set; } = TaskStatus.Default;
    [Column("TaskGroup")] public int GroupId { get; set; }

    [PrimaryKey]
    [AutoIncrement]
    [Column("TaskId")]
    public int PrimaryKey { get; set; }

    internal static BackLog CreateNew(string title, string? description = null, BackGroup? group = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        group ??= BackGroup.Default;
        return new BackLog
        {
            Title = title,
            Description = description ?? string.Empty,
            GroupId = group.PrimaryKey
        };
    }
}

[Serializable]
public enum TaskStatus
{
    /// <summary>
    ///     默认状态
    /// </summary>
    Default,

    /// <summary>
    ///     正在处理
    /// </summary>
    InProgress,

    /// <summary>
    ///     成功完成
    /// </summary>
    Completed,

    /// <summary>
    ///     失败
    /// </summary>
    Failed,

    /// <summary>
    ///     忽略
    /// </summary>
    Ignored
}