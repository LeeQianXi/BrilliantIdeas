namespace DeadLine.Core.Abstract.Controls;

public partial class DeadLineItem : TemplatedControl, ICoroutinator
{
    public static readonly DirectProperty<DeadLineItem, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<DeadLineItem, double>(nameof(Progress), o => o.Progress);

    private readonly Coroutine _coroutine;

    public DeadLineItem()
    {
        _coroutine = this.StartCoroutine(ProgressUpdate, false);
    }

    [GeneratedStyledProperty] public partial string Title { get; set; }
    [GeneratedStyledProperty] public partial DateTime StartTime { get; set; }
    [GeneratedStyledProperty] public partial DateTime EndTime { get; set; }

    public double Progress
    {
        get;
        private set
        {
            if (Math.Abs(value - field) < 0.0001) return;
            SetAndRaise(ProgressProperty, ref field, value);
        }
    }

    public CancellationTokenSource CancellationTokenSource { get; } = new();

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
            if (now < StartTime) return 0;
            if (now > EndTime) return 1;
            return (now - StartTime) / space;
        }
    }
}