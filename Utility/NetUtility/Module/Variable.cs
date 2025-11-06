namespace NetUtility.Module;

/// <summary>
///     变量。
/// </summary>
public abstract class Variable : IReference
{
    /// <summary>
    ///     获取变量类型。
    /// </summary>
    public abstract Type Type { get; }

    /// <summary>
    ///     清理变量值。
    /// </summary>
    public abstract void Reset();

    /// <summary>
    ///     获取变量值。
    /// </summary>
    /// <returns>变量值。</returns>
    public abstract object? GetValue();

    /// <summary>
    ///     设置变量值。
    /// </summary>
    /// <param name="value">变量值。</param>
    public abstract void SetValue(object? value);
}