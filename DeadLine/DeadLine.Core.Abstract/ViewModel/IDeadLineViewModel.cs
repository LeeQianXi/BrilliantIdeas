using NetUtility;

namespace DeadLine.Core.Abstract.ViewModel;

public interface IDeadLineViewModel : IDependencyInjection
{
    Interaction<INewDeadLineItemView, DeadLineItemInfo?> ShowDialogInteraction { get; }
    Interaction<IEditItemInfoWindow, ConsumeFactory<DeadLineItemInfo>?> EditItemInfoInteraction { get; }
    IAsyncRelayCommand NewDeadLineItemCommand { get; }
    IRelayCommand<DeadLineItemInfo> DisplayDeadLineItemCommand { get; }
    IRelayCommand SaveToDatabaseCommand { get; }
    IObservable<IChangeSet<DeadLineItemInfo>> DeadLineItemsConnect();
    IAsyncEnumerable<DeadLineItemInfo> LoadDatabase();
}