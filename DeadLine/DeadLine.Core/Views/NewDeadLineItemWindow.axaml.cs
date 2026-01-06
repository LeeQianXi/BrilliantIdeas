using Avalonia.Controls.Notifications;

namespace DeadLine.Core.Views;

public partial class NewDeadLineItemWindow : ViewModelWindowBase<INewDeadLineItemViewModel>, INewDeadLineItemView
{
    public NewDeadLineItemWindow()
    {
        InitializeComponent();
        ViewModel!.CloseIteration.RegisterHandler(OnInteractionClose);
        ViewModel!.DisplayWarningInteraction.RegisterHandler(OnInteractionDisplayWarning);
    }

    private void OnInteractionDisplayWarning(IInteractionContext<INotification, Unit> context)
    {
        Notification.Show(context.Input);
        context.SetOutput(Unit.Default);
    }

    private void OnInteractionClose(IInteractionContext<DeadLineItemInfo?, Unit> context)
    {
        Close(context.Input);
        context.SetOutput(Unit.Default);
    }
}