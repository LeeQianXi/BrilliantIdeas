namespace DeadLine.Core.Views;

public partial class DeadLineWindow : ViewModelWindowBase<IDeadLineViewModel>, IStartupWindow, IDeadLineView,
    ICoroutinator
{
    private static readonly FilterComboBoxItem[] FilterComboBoxItems =
    [
        new(DeadLineFilterType.All, "所有"),
        new(DeadLineFilterType.ToDo, "未开始"),
        new(DeadLineFilterType.Doing, "进行中"),
        new(DeadLineFilterType.Done, "已完成"),
        new(DeadLineFilterType.Failed, "已失败"),
        new(DeadLineFilterType.TimedOut, "已超时")
    ];

    private readonly Coroutine _coroutine;

    private bool _isClosed;

    public DeadLineWindow()
    {
        InitializeComponent();
        FilterComboBox.ItemsSource = FilterComboBoxItems;
        FilterComboBox.SelectedItem = FilterComboBoxItems.First();
        ViewModel!.ShowDialogInteraction.RegisterHandler(ShowDialogInteraction);
        var filterObservable = this
            .WhenAnyValue(x => x.FilterComboBox.SelectedItem)
            .Select(item => item is FilterComboBoxItem filter ? filter.FilterType : DeadLineFilterType.All)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Select(FilterInteraction);
        ViewModel!.DeadLineItemsConnect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(filterObservable)
            .Bind(out var displayedDeadLineItems)
            .Subscribe();
        DeadLineListBox.ItemsSource = displayedDeadLineItems;
        this.StartCoroutine(LoadExistedDeadLineItems);
        _coroutine = this.StartCoroutine(TimeSpanSave);
    }

    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    private async Task ShowDialogInteraction(IInteractionContext<INewDeadLineItemView, DeadLineItemInfo?> arg)
    {
        arg.SetOutput(await arg.Input.ShowDialog<DeadLineItemInfo>(this));
    }

    private static Func<DeadLineItemInfo, bool> FilterInteraction(DeadLineFilterType filter)
    {
        return info => filter is DeadLineFilterType.All || (int)info.Status == (int)filter;
    }

    private IEnumerator<YieldInstruction?> LoadExistedDeadLineItems()
    {
        foreach (var item in ViewModel!.LoadDeadLineItems(CoroutinatorCancelTokenSource.Token))
        {
            ViewModel!.AddDeadLineItemCommand.Execute(item);
            yield return null;
        }
    }

    private IEnumerator<YieldInstruction?> TimeSpanSave()
    {
        while (!_isClosed)
        {
            yield return new WaitForSeconds(TimeSpan.FromMinutes(5));
            ViewModel!.SaveDeadLineItemsCommand.Execute(null);
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _isClosed = true;
        _coroutine.Close();
        ViewModel!.SaveDeadLineItemsCommand.Execute(null);
    }
}

internal enum DeadLineFilterType
{
    All = -1,
    ToDo = DeadLineStatus.ToDo,
    Doing = DeadLineStatus.Doing,
    Done = DeadLineStatus.Done,
    Failed = DeadLineStatus.Failed,
    TimedOut = DeadLineStatus.TimedOut
}

internal record FilterComboBoxItem(DeadLineFilterType FilterType, string Text);