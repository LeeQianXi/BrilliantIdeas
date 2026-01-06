namespace DeadLine.DataBase.Core.Model;

[Table(nameof(DeadLineItemInfo))]
public class DeadLineItemInfo(string title, DateTime startTime, DateTime endTime) : IModelBasic, INotifyPropertyChanged
{
    public DeadLineItemInfo() : this("Default", DateTime.MinValue, DateTime.MaxValue)
    {
    }

    [Ignore] public Action<DeadLineItemInfo>? RemoveClickEventHandler { get; set; }

    [Ignore] public Action<DeadLineItemInfo>? EditClickEventHandler { get; set; }

    public DeadLineStatus Status
    {
        get;
        set => SetField(ref field, value);
    } = DeadLineStatus.ToDo;

    public string Title
    {
        get;
        set => SetField(ref field, value);
    } = title;

    public string Description
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public DateTime StartTime { get; init; } = startTime;
    public DateTime EndTime { get; init; } = endTime;

    [PrimaryKey] [AutoIncrement] public int PrimaryKey { get; set; } = -1;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event Action<DeadLineItemInfo> RemoveClickEvent
    {
        add => RemoveClickEventHandler += value;
        remove => RemoveClickEventHandler -= value;
    }

    public event Action<DeadLineItemInfo> EditClickEvent
    {
        add => EditClickEventHandler += value;
        remove => EditClickEventHandler -= value;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void Deconstruct(out string title, out DateTime startTime, out DateTime endTime)
    {
        title = Title;
        startTime = StartTime;
        endTime = EndTime;
    }

    public override string ToString()
    {
        return $"DeadLine[Title={Title}, StartTime={StartTime}, EndTime={EndTime}]";
    }
}