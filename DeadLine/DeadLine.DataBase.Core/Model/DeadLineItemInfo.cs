namespace DeadLine.DataBase.Core.Model;

[Table(nameof(DeadLineItemInfo))]
public class DeadLineItemInfo : IModelBasic, INotifyPropertyChanged
{
    public DeadLineItemInfo()
    {
        Title = "Default";
        StartTime = DateTime.MinValue;
        EndTime = DateTime.MaxValue;
    }

    public DeadLineItemInfo(string title, DateTime startTime, DateTime endTime)
    {
        Title = title;
        StartTime = startTime;
        EndTime = endTime;
    }

    public DeadLineStatus Status
    {
        get;
        set => SetField(ref field, value);
    } = DeadLineStatus.ToDo;

    public string Title { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }

    [PrimaryKey] [AutoIncrement] public int PrimaryKey { get; set; } = -1;

    public event PropertyChangedEventHandler? PropertyChanged;

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
}