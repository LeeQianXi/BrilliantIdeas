using Avalonia.Controls.Notifications;

namespace DeadLine.Core.Abstract.ViewModel;

public interface INewDeadLineItemViewModel : IDependencyInjection, IReactiveObject
{
    IAsyncRelayCommand CancelCommand { get; }
    IAsyncRelayCommand CreateItemCommand { get; }
    string Title { get; set; }
    string Description { get; set; }
    DateTime StartDate { get; set; }
    DateTime EndDate { get; set; }
    TimeSpan StartTime { get; set; }
    TimeSpan EndTime { get; set; }
    Interaction<DeadLineItemInfo?, Unit> CloseIteration { get; }
    Interaction<INotification, Unit> DisplayWarningInteraction { get; }
}