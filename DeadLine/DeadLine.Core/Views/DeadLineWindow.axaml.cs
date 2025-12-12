using DynamicData.Alias;

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
        new(DeadLineFilterType.TimedOut, "已超时")
    ];

    private readonly Coroutine? _loadingCoroutine;
    private readonly Coroutine? _saveCoroutine;

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

        _loadingCoroutine = this.StartCoroutine(LoadExistedDeadLineItems);
        _saveCoroutine = this.StartCoroutine(TimeSpanSave);
    }

    public CancellationTokenSource CoroutinatorCancelTokenSource { get; } = new();

    private async Task ShowDialogInteraction(IInteractionContext<INewDeadLineItemView, DeadLineItemInfo?> arg)
    {
        arg.SetOutput(await arg.Input.ShowDialog<DeadLineItemInfo>(this));
    }

    private static Func<DeadLineItemInfo, bool> FilterFlagInteraction(DeadLineFilterType filter)
    {
        return info => filter is DeadLineFilterType.All || (int)info.Status == (int)filter;
    }

    private static Func<DeadLineItemInfo, bool> FilterTextInteraction(string filter)
    {
        return text => string.IsNullOrWhiteSpace(filter) ||
                       text.Title.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    private async IAsyncEnumerator<YieldInstruction?> LoadExistedDeadLineItems()
    {
        await foreach (var item in ViewModel!.LoadDeadLineItems())
        {
            ViewModel!.AddDeadLineItemCommand.Execute(item);
            yield return null;
        }
    }

    private IEnumerator<YieldInstruction?> TimeSpanSave()
    {
        yield return new WaitForSeconds(TimeSpan.FromMinutes(5));
        while (!_isClosed)
        {
            ViewModel!.SaveDeadLineItemsCommand.Execute(null);
            yield return new WaitForSeconds(TimeSpan.FromMinutes(5));
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _isClosed = true;
        _saveCoroutine?.Close();
        ViewModel!.SaveDeadLineItemsCommand.Execute(null);
    }
}

internal enum DeadLineFilterType
{
    All = -1,
    ToDo = DeadLineStatus.ToDo,
    Doing = DeadLineStatus.Doing,
    Done = DeadLineStatus.Done,
    TimedOut = DeadLineStatus.TimedOut
}

internal record FilterComboBoxItem(DeadLineFilterType FilterType, string Text);