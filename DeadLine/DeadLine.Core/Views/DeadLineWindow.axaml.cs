namespace DeadLine.Core.Views;

public partial class DeadLineWindow : ViewModelWindowBase<IDeadLineViewModel>, IStartupWindow, IDeadLineView,
    ICoroutinator
{
    private static readonly FilterComboBoxItem[] FilterComboBoxItems =
    [
        new(-1, "所有"),
        new((int)DeadLineStatus.ToDo, "未开始"),
        new((int)DeadLineStatus.Doing, "进行中"),
        new((int)DeadLineStatus.Done, "已完成"),
        new((int)DeadLineStatus.TimedOut, "已超时")
    ];

    private readonly Coroutine? _loadingCoroutine;
    private readonly Coroutine? _saveCoroutine;

    public DeadLineWindow()
    {
        InitializeComponent();
        //初始化过滤选项
        FilterComboBox.ItemsSource = FilterComboBoxItems;
        FilterComboBox.SelectedIndex = 0;
        //注册交互
        ViewModel!.ShowDialogInteraction.RegisterHandler(ShowDialogInteraction);
        ViewModel!.EditItemInfoInteraction.RegisterHandler(EditItemInfoInteraction);
        //链接过滤项
        var filterObservable = this
            .WhenAnyValue(x => x.FilterComboBox.SelectedItem)
            .Select(item => item is FilterComboBoxItem filter ? filter.DeadLineStatus : -1)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Select(FilterFlagInteraction);
        var filterTextObservable = this
            .WhenAnyValue(x => x.FilterTextBox.Text)
            .Select(text => text ??= string.Empty)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Select(FilterTextInteraction);
        ViewModel!.DeadLineItemsConnect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(filterObservable)
            .Filter(filterTextObservable)
            .Bind(out var displayedDeadLineItems)
            .Subscribe();
        ViewModel!.DeadLineItemsConnect()
            .Select(set => set.Title)
            .Bind(out var filtercomboboxitems)
            .Subscribe();
        DeadLineListBox.ItemsSource = displayedDeadLineItems;
        FilterTextBox.ItemsSource = filtercomboboxitems;
        //启动协程加载
        _loadingCoroutine = this.StartCoroutine(LoadExistedDeadLineItems);
        _saveCoroutine = this.StartCoroutine(TimeSpanSave);
    }

    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    private async Task ShowDialogInteraction(IInteractionContext<INewDeadLineItemView, DeadLineItemInfo?> context)
    {
        var deadLineItemInfo = await context.Input.ShowDialog<DeadLineItemInfo>(this);
        context.SetOutput(deadLineItemInfo);
    }

    private async Task EditItemInfoInteraction(
        IInteractionContext<IEditItemInfoWindow, ConsumeFactory<DeadLineItemInfo>?> context)
    {
        var consume = await context.Input.ShowDialog<ConsumeFactory<DeadLineItemInfo>>(this);
        context.SetOutput(consume);
    }

    private static Func<DeadLineItemInfo, bool> FilterFlagInteraction(int status)
    {
        return info => status is -1 || (int)info.Status == status;
    }

    private static Func<DeadLineItemInfo, bool> FilterTextInteraction(string filter)
    {
        return text => string.IsNullOrWhiteSpace(filter) ||
                       text.Title.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    private async IAsyncEnumerator<YieldInstruction?> LoadExistedDeadLineItems()
    {
        await foreach (var item in ViewModel!.LoadDatabase())
        {
            ViewModel!.DisplayDeadLineItemCommand.Execute(item);
            yield return null;
        }
    }

    private IEnumerator<YieldInstruction?> TimeSpanSave(CancellationToken token)
    {
        yield return new WaitForSeconds(TimeSpan.FromMinutes(5));
        while (!token.IsCancellationRequested)
        {
            ViewModel!.SaveToDatabaseCommand.Execute(null);
            yield return new WaitForSeconds(TimeSpan.FromMinutes(5));
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        CoroutinatorCancelTokenSource.Cancel();
        ViewModel!.SaveToDatabaseCommand.Execute(null);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
    }
}

internal record FilterComboBoxItem(int DeadLineStatus, string Text)
{
    public override string ToString()
    {
        return Text;
    }
}