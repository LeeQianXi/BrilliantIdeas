namespace DeadLine.Core.Abstract.ViewModel;

public interface IDeadLineViewModel : IDependencyInjection
{
    Window? Owner { get; set; }
    IAsyncRelayCommand NewDeadLineItemCommand { get; }
    Interaction<INewDeadLineItemView, string> ShowDialogInteraction { get; }
}