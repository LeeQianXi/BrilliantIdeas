namespace DeadLine.Core.ViewModel;

public partial class DeadLineViewModel(IServiceProvider serviceProvider) : ViewModelBase, IDeadLineViewModel
{
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<DeadLineViewModel>>();

    public Window? Owner { get; set; }

    public Interaction<INewDeadLineItemView, string> ShowDialogInteraction { get; } = new();

    [RelayCommand]
    private async Task NewDeadLineItem()
    {
        var niv = ServiceProvider.GetRequiredService<INewDeadLineItemView>();
        var obj = await ShowDialogInteraction.Handle(niv);
        Logger.LogInformation(obj);
    }
}