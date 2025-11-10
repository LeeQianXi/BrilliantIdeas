namespace DeadLine.Core.Abstract.ViewModel;

public interface IDeadLineViewModel : IDependencyInjection
{
    /// <summary>
    ///     按键绑定
    ///     新建任务
    /// </summary>
    IAsyncRelayCommand NewDeadLineItemCommand { get; }

    /// <summary>
    ///     交互
    ///     新任务具体信息
    /// </summary>
    Interaction<INewDeadLineItemView, DeadLineItemInfo?> ShowDialogInteraction { get; }

    IAsyncRelayCommand<DeadLineItemInfo> AddDeadLineItemCommand { get; }
    IAsyncRelayCommand<DeadLineItemInfo> RemoveDeadLineItemCommand { get; }
    IAsyncRelayCommand SaveDeadLineItemsCommand { get; }
    IEnumerable<DeadLineItemInfo> LoadDeadLineItems(CancellationToken token);
    IObservable<IChangeSet<DeadLineItemInfo>> DeadLineItemsConnect();
}