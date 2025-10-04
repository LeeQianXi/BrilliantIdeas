using SQLite;

namespace ToDoListDb.Abstract.Model;

[Table(nameof(BackLog))]
public class BackLog
{
    [PrimaryKey, AutoIncrement, Column("TaskId")]
    public int TaskId { get; set; }

    [Column("Title")] public string Title { get; set; } = string.Empty;
    [Column("Description")] public string Description { get; set; } = string.Empty;
    [Column("TaskStatus")] public TaskStatus Status { get; set; } = TaskStatus.Default;
    [Column("TaskGroup")] public int GroupId { get; set; }

    public static BackLog CreateNew(string title, string? description = null, BackGroup? group = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        group ??= BackGroup.Default;
        return new BackLog
        {
            Title = title,
            Description = description ?? string.Empty,
            GroupId = group.GroupId,
        };
    }

    public static readonly BackLog Empty = new() { Title = "Empty", TaskId = -1 };
}

[Serializable]
public enum TaskStatus
{
    /// <summary>
    /// 默认状态
    /// </summary>
    Default,
    /// <summary>
    /// 正在处理
    /// </summary>
    InProgress,
    /// <summary>
    /// 成功完成
    /// </summary>
    Completed,
    /// <summary>
    /// 失败
    /// </summary>
    Failed,
    /// <summary>
    /// 忽略
    /// </summary>
    Ignored,
}