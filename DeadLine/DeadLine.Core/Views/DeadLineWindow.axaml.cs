namespace DeadLine.Core.Views;

public partial class DeadLineWindow : ViewModelWindowBase<IDeadLineViewModel>, IStartupWindow, IDeadLineView
{
    public DeadLineWindow()
    {
        InitializeComponent();
        ViewModel.ShowDialogInteraction.RegisterHandler(ShowDialogInteraction);
        var dlis = new List<DeadLineItem>();
        for (var i = 0; i < 100; i++)
            dlis.Add(new DeadLineItem
            {
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                EndTime = DateTime.UtcNow.AddMinutes(5)
            });

        ListBox.ItemsSource = dlis;
    }

    private async Task ShowDialogInteraction(IInteractionContext<INewDeadLineItemView, string> arg)
    {
        var ret = await arg.Input.ShowDialog<string>(this);
        arg.SetOutput(ret);
    }
}