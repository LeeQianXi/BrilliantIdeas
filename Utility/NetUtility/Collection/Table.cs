namespace NetUtility.Collection;

/// <summary>
///     强类型二维表接口
/// </summary>
public interface ITable<TColumn, TRow, TValue>
    where TColumn : notnull
    where TRow : notnull
{
    TValue this[TColumn column, TRow row] { get; set; }
    IEnumerable<TColumn> Columns { get; }
    IEnumerable<TRow> Rows { get; }
    bool ContainsColumn(TColumn column);
    bool ContainsRow(TRow row);
    bool TryGetValue(TColumn column, TRow row, out TValue value);
    void Clear();
}

/// <summary>
///     默认实现：内部用嵌套字典存储
/// </summary>
public sealed class Table<TColumn, TRow, TValue> : ITable<TColumn, TRow, TValue>
    where TColumn : notnull
    where TRow : notnull
{
    private readonly Dictionary<TColumn, Dictionary<TRow, TValue>> _data = new();

    public TValue this[TColumn column, TRow row]
    {
        get
        {
            if (_data.TryGetValue(column, out var rowDict) &&
                rowDict.TryGetValue(row, out var value))
                return value!;
            throw new KeyNotFoundException($"[{column},{row}] Not found in Table");
        }
        set
        {
            if (!_data.TryGetValue(column, out var rowDict))
                _data[column] = rowDict = new Dictionary<TRow, TValue>();
            rowDict[row] = value;
        }
    }

    public IEnumerable<TColumn> Columns => _data.Keys;
    public IEnumerable<TRow> Rows => _data.Values.SelectMany(d => d.Keys).Distinct();

    public bool ContainsColumn(TColumn column)
    {
        return _data.ContainsKey(column);
    }

    public bool ContainsRow(TRow row)
    {
        return _data.Values.Any(d => d.ContainsKey(row));
    }

    public bool TryGetValue(TColumn column, TRow row, out TValue value)
    {
        value = default!;
        if (_data.TryGetValue(column, out var rowDict))
            return rowDict.TryGetValue(row, out value!);
        return false;
    }

    public void Clear()
    {
        foreach (var dic in _data.Values) dic.Clear();

        _data.Clear();
    }
}