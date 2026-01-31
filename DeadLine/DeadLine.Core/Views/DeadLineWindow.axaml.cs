using Avalonia.Controls.Notifications;
using AvaloniaUtility.Controls;
using NetUtility.RefPool;

namespace DeadLine.Core.Views;

public partial class DeadLineWindow : ViewModelWindowBase<IDeadLineViewModel>, IStartupWindow, IDeadLineWindow,
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

    public DeadLineWindow()
    {
        InitializeComponent();

        #region MainPage

        //初始化过滤选项
        FilterComboBox.ItemsSource = FilterComboBoxItems;
        FilterComboBox.SelectedIndex = 0;
        //注册交互
        ViewModel!.ShowDialogInteraction.RegisterHandler(ShowDialogInteraction);
        ViewModel!.EditItemInfoInteraction.RegisterHandler(EditItemInfoInteraction);
        ViewModel!.NotifyScreenInteraction.RegisterHandler(NotifyScreenInteraction);
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
        var changes = ViewModel!.DeadLineItemsConnect();
        changes
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(filterObservable)
            .Filter(filterTextObservable)
            .Bind(out var displayedDeadLineItems)
            .Subscribe();
        changes
            .Select(set => set.Title)
            .Bind(out var filtercomboboxitems)
            .Subscribe();
        DeadLineListBox.ItemsSource = displayedDeadLineItems;
        FilterTextBox.ItemsSource = filtercomboboxitems;
        //启动协程加载
        this.StartCoroutine(LoadExistedDeadLineItems);
        //this.StartCoroutine(TimeSpanSave);

        #endregion

        #region Statistical

        changes
            .Select(set => set.Count);

        #endregion
    }


    public CancellationTokenSource CoroutineCancelTokenSource { get; } = new();

    private async Task ShowDialogInteraction(IInteractionContext<INewDeadLineItemView, DeadLineItemInfo?> context)
    {
        Show();
        var deadLineItemInfo = await context.Input.ShowDialog<DeadLineItemInfo>(this);
        context.SetOutput(deadLineItemInfo);
    }

    private async Task EditItemInfoInteraction(
        IInteractionContext<IEditItemInfoWindow, ConsumeFactory<DeadLineItemInfo>?> context)
    {
        var consume = await context.Input.ShowDialog<ConsumeFactory<DeadLineItemInfo>>(this);
        context.SetOutput(consume);
    }

    private void NotifyScreenInteraction(IInteractionContext<INotification, Unit> context)
    {
        Manager.Show(context.Input);
        context.SetOutput(Unit.Default);
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

    private async IAsyncEnumerator<YieldInstruction?> LoadExistedDeadLineItems(CancellationToken token)
    {
        await foreach (var item in ViewModel!.LoadDatabase(token))
        {
            ViewModel!.DisplayDeadLineItemCommand.Execute(item);
            yield return null;
        }

        var notification = ReferenceNotification.AcquireReference();
        notification.Init("系统消息", "成功加载任务信息", NotificationType.Success);
        await ViewModel!.NotifyScreenInteraction.Handle(notification);
    }

    private async IAsyncEnumerator<YieldInstruction?> TimeSpanSave(CancellationToken token)
    {
        yield return new WaitForSeconds(TimeSpan.FromMinutes(5));
        while (!token.IsCancellationRequested)
        {
            ViewModel!.SaveToDatabaseCommand.Execute(null);
            var notification = ReferenceNotification.AcquireReference();
            notification.Init("系统消息", "自动保存已完成", NotificationType.Success);
            await ViewModel!.NotifyScreenInteraction.Handle(notification);

            yield return new WaitForSeconds(TimeSpan.FromMinutes(5));
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        CoroutineCancelTokenSource.Cancel();
        ViewModel!.SaveToDatabaseCommand.Execute(null);
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