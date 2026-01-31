using Avalonia.Controls.Notifications;
using DIAbstract;
using LiveChartsCore.SkiaSharpView;
using NetUtility;

namespace DeadLine.Core.Abstract.ViewModel;

public interface IDeadLineViewModel : IDependencyInjection
{
    #region MainPage

    Interaction<INewDeadLineItemView, DeadLineItemInfo?> ShowDialogInteraction { get; }
    Interaction<IEditItemInfoWindow, ConsumeFactory<DeadLineItemInfo>?> EditItemInfoInteraction { get; }
    Interaction<INotification, Unit> NotifyScreenInteraction { get; }
    IAsyncRelayCommand NewDeadLineItemCommand { get; }
    IRelayCommand<DeadLineItemInfo> DisplayDeadLineItemCommand { get; }
    IRelayCommand SaveToDatabaseCommand { get; }
    IObservable<IChangeSet<DeadLineItemInfo>> DeadLineItemsConnect();
    IAsyncEnumerable<DeadLineItemInfo> LoadDatabase(CancellationToken token = default);
    void RefreshData();

    #endregion

    #region Statistical

    int TotalCount { get; }
    int DoneCount { get; }
    int DoingCount { get; }
    int ToDoCount { get; }
    int TimedOutCount { get; }
    int DueTodayCount { get; }
    int DueWeekCount { get; }
    int NotCompletedCount { get; }
    IEnumerable<PieSeries<int>> Series { get; }

    #endregion
}