namespace DeadLine.Core.Views;

public partial class DeadLineWindow : ViewModelWindowBase<IDeadLineViewModel>, IStartupWindow, IDeadLineView
{
    public DeadLineWindow()
    {
        InitializeComponent();
        ViewModel!.ShowDialogInteraction.RegisterHandler(ShowDialogInteraction);
        var dlis = new List<DeadLineItem>();
        for (var i = 0; i < 20; i++)
            dlis.Add(new DeadLineItem
            {
                Title = "Item " + i,
                StartTime = DateTime.UtcNow.AddSeconds(Random.Shared.Next(-10, 10)),
                EndTime = DateTime.UtcNow.AddSeconds(Random.Shared.Next(11, 60))
            });

        ListBox.ItemsSource = dlis;
    }

    private async Task ShowDialogInteraction(IInteractionContext<INewDeadLineItemView, string> arg)
    {
        var ret = await arg.Input.ShowDialog<string>(this);
        arg.SetOutput(ret);
    }
}