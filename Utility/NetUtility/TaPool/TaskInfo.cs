namespace NetUtility.TaPool;

/// <summary>
///     任务信息。
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct TaskInfo
{
    /// <summary>
    ///     初始化任务信息的新实例。
    /// </summary>
    /// <param name="serialId">任务的序列编号。</param>
    /// <param name="tag">任务的标签。</param>
    /// <param name="priority">任务的优先级。</param>
    /// <param name="userData">任务的用户自定义数据。</param>
    /// <param name="status">任务状态。</param>
    /// <param name="description">任务描述。</param>
    public TaskInfo(int serialId, string? tag, int priority, object? userData, TaskStatus status, string? description)
    {
        IsValid = true;
        SerialId = serialId;
        Tag = tag;
        Priority = priority;
        UserData = userData;
        Status = status;
        Description = description;
    }

    /// <summary>
    ///     获取任务信息是否有效。
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    ///     获取任务的序列编号。
    /// </summary>
    public int SerialId => !IsValid ? throw new NetUtilityException("Data is invalid.") : field;

    /// <summary>
    ///     获取任务的标签。
    /// </summary>
    public string? Tag => !IsValid ? throw new NetUtilityException("Data is invalid.") : field;

    /// <summary>
    ///     获取任务的优先级。
    /// </summary>
    public int Priority => !IsValid ? throw new NetUtilityException("Data is invalid.") : field;

    /// <summary>
    ///     获取任务的用户自定义数据。
    /// </summary>
    public object? UserData => !IsValid ? throw new NetUtilityException("Data is invalid.") : field;

    /// <summary>
    ///     获取任务状态。
    /// </summary>
    public TaskStatus Status => !IsValid ? throw new NetUtilityException("Data is invalid.") : field;

    /// <summary>
    ///     获取任务描述。
    /// </summary>
    public string? Description => !IsValid ? throw new NetUtilityException("Data is invalid.") : field;
}