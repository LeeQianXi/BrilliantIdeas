namespace DeadLine.Core.Abstract.Controls;

[TemplatePart("PART_Progress", typeof(ProgressBar))]
[TemplatePart("PART_Tag", typeof(Label))]
public partial class DeadLineItem : TemplatedControl, ICoroutinator
{
    public static readonly DirectProperty<DeadLineItem, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<DeadLineItem, double>(nameof(Progress), o => o.Progress);

    private readonly Coroutine _coroutine;

    private ProgressBar? _partProgressBar;
    private Label? _partTag;

    public DeadLineItem()
    {
        _coroutine = this.StartCoroutine(ProgressUpdate, false);
    }

    [GeneratedStyledProperty] public partial string Title { get; set; }
    [GeneratedStyledProperty] public partial DateTime StartTime { get; set; }
    [GeneratedStyledProperty] public partial DateTime EndTime { get; set; }
    [GeneratedStyledProperty] public partial DeadLineStatus Status { get; set; }

    public double Progress
    {
        get;
        private set
        {
            OnProgressChanged(value);
            SetAndRaise(ProgressProperty, ref field, value);
        }
    }

    public CancellationTokenSource CancellationTokenSource { get; } = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _partProgressBar = e.NameScope.Find<ProgressBar>("PART_Progress")!;
        _partTag = e.NameScope.Find<Label>("PART_Tag");
        OnStatusPropertyChanged(Status);
        OnProgressChanged(Progress);
    }

    partial void OnProgressChanged(double progress);

    partial void OnProgressChanged(double progress)
    {
        if (_partProgressBar is null) return;
        _partProgressBar.Classes.Clear();
        if (Status is DeadLineStatus.TimedOut or DeadLineStatus.Failed)
        {
            _partProgressBar.Classes.Add("Error");
            return;
        }

        switch (progress)
        {
            case >= 1d:
                _partProgressBar.Classes.Add("Primary");
                break;
            case > 0.3d:
                _partProgressBar.Classes.Add("Secondary");
                break;
            case > 0.1d:
                _partProgressBar.Classes.Add("Warning");
                break;
            default:
                _partProgressBar.Classes.Add("Error");
                break;
        }
    }

    partial void OnStatusPropertyChanged(DeadLineStatus newValue)
    {
        if (_partTag is null) return;
        _partTag.Classes.Clear();
        _partTag.Classes.Add("Ghost");
        switch (newValue)
        {
            case DeadLineStatus.ToDo:
                _partTag.Classes.Add("Grey");
                break;
            case DeadLineStatus.Doing:
                _partTag.Classes.Add("LightBlue");
                break;
            case DeadLineStatus.Failed:
            case DeadLineStatus.TimedOut:
                _partTag.Classes.Add("Red");
                break;
            case DeadLineStatus.Done:
                _partTag.Classes.Add("Green");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _coroutine.Continue();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _coroutine.Stop();
        base.OnUnloaded(e);
    }

    private IEnumerator<YieldInstruction?> ProgressUpdate()
    {
        var space = EndTime - StartTime;
        do
        {
            Progress = CalcDuring();
            yield return null;
        } while (true);

        double CalcDuring()
        {
            var now = DateTime.UtcNow;
            if (now < StartTime)
            {
                Status = DeadLineStatus.ToDo;
                return 1;
            }

            if (now > EndTime)
            {
                if (Status is DeadLineStatus.Doing)
                    Status = DeadLineStatus.TimedOut;
                return 0;
            }

            if (Status is DeadLineStatus.ToDo)
                Status = DeadLineStatus.Doing;
            return (EndTime - now) / space;
        }
    }
}

public readonly record struct DeadLineInfo(DateTime StartTime, DateTime EndTime, DeadLineStatus Status)
{
    public string Title { get; init; } = "新建任务";
}