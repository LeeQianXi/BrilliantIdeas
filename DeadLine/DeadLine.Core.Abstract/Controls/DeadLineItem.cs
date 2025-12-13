namespace DeadLine.Core.Abstract.Controls;

[TemplatePart("PART_Progress", typeof(ProgressBar))]
[TemplatePart("PART_Tag", typeof(Label))]
[TemplatePart("PART_DoneWork", typeof(CheckBox))]
[TemplatePart("PART_Remove", typeof(Button))]
public partial class DeadLineItem : TemplatedControl, ICoroutinator
{
    private readonly Coroutine _coroutine;
    private CheckBox? _partDoneWork;
    private ProgressBar? _partProgressBar;
    private Button? _partRemove;
    private Label? _partTag;

    public DeadLineItem()
    {
        _coroutine = this.StartCoroutine(ProgressUpdate, false);
    }

    [GeneratedStyledProperty] public partial string Title { get; set; }
    [GeneratedStyledProperty] public partial string Description { get; set; }
    [GeneratedStyledProperty] public partial DateTime StartTime { get; set; }
    [GeneratedStyledProperty] public partial DateTime EndTime { get; set; }
    [GeneratedStyledProperty] public partial DeadLineStatus Status { get; set; }
    [GeneratedDirectProperty] public partial double Progress { get; set; }
    [GeneratedDirectProperty] public partial bool WithDescription { get; set; }
    [GeneratedDirectProperty] public partial TimeSpan RemainingTime { get; set; }
    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _partProgressBar = e.NameScope.Find<ProgressBar>("PART_Progress")!;
        _partTag = e.NameScope.Find<Label>("PART_Tag");
        _partDoneWork = e.NameScope.Find<CheckBox>("PART_DoneWork");
        _partRemove = e.NameScope.Find<Button>("PART_Remove");
        _partRemove!.Click += OnRemoveClick;
        _partDoneWork!.IsCheckedChanged += OnDongWorkChanged;
        OnStatusPropertyChanged(Status);
        OnProgressPropertyChanged(Progress);
    }

    partial void OnDescriptionPropertyChanged(string newValue)
    {
        WithDescription = !string.IsNullOrWhiteSpace(newValue);
    }

    private void OnRemoveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not DeadLineItemInfo info) return;
        info.RemoveClickEvent?.Invoke(info);
    }

    private void OnDongWorkChanged(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (_partDoneWork is null) return;
        if (!_partDoneWork.IsChecked!.Value) return;
        Status = DeadLineStatus.Done;
        Progress = 1;
    }

    partial void OnProgressPropertyChanged(double newValue)
    {
        if (_partProgressBar is null) return;
        _partProgressBar.Classes.Clear();
        switch (Status)
        {
            case DeadLineStatus.TimedOut:
                _partProgressBar.Classes.Add("Error");
                return;
            case DeadLineStatus.Done:
                _partProgressBar.Classes.Add("Success");
                return;
            case DeadLineStatus.ToDo:
            case DeadLineStatus.Doing:
            default:
                switch (newValue)
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
            case DeadLineStatus.TimedOut:
                _partTag.Classes.Add("Red");
                break;
            case DeadLineStatus.Done:
                _partTag.Classes.Add("Green");
                _partDoneWork!.IsChecked = true;
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
        } while (Status is not (DeadLineStatus.Done or DeadLineStatus.TimedOut));

        yield break;

        double CalcDuring()
        {
            if (Status is DeadLineStatus.Done or DeadLineStatus.TimedOut)
            {
                RemainingTime = TimeSpan.Zero;
                return 1;
            }

            var now = DateTime.Now;
            if (now < StartTime)
            {
                Status = DeadLineStatus.ToDo;
                RemainingTime = space;
                return 1;
            }

            if (now > EndTime)
            {
                if (Status is not DeadLineStatus.Done)
                    Status = DeadLineStatus.TimedOut;
                RemainingTime = TimeSpan.Zero;
                return 1;
            }

            if (Status is DeadLineStatus.ToDo)
                Status = DeadLineStatus.Doing;
            RemainingTime = EndTime - now;
            return RemainingTime / space;
        }
    }
}