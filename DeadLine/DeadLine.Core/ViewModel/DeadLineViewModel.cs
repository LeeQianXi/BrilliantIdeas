namespace DeadLine.Core.ViewModel;

public partial class DeadLineViewModel(IServiceProvider serviceProvider)
    : ViewModelBase, IDeadLineViewModel
{
    private readonly IDeadLineInfoStorage _storage = serviceProvider.GetRequiredService<IDeadLineInfoStorage>();
    private bool _isLoaded;
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<DeadLineViewModel>>();

    public Interaction<INewDeadLineItemView, DeadLineItemInfo?> ShowDialogInteraction { get; } = new();
    public AvaloniaList<DeadLineItemInfo> DeadLineItems { get; } = [];

    public IEnumerable<DeadLineItemInfo> LoadDeadLineItems(CancellationToken token)
    {
        if (_isLoaded)
            yield break;
        IEnumerable<DeadLineItemInfo> items = [];
        var completed = false;
        _storage.BeginTransactionAsync(con =>
        {
            items = items.Concat(con.Table<DeadLineItemInfo>().ToList());
            completed = true;
        });
        while (!completed) ;
        foreach (var item in items)
            yield return item;
        _isLoaded = true;
    }

    [RelayCommand]
    private async Task NewDeadLineItem()
    {
        var niv = ServiceProvider.GetRequiredService<INewDeadLineItemView>();
        var lii = await ShowDialogInteraction.Handle(niv);
        if (lii is null) return;
        Logger.LogInformation("Success Get new deadline item {lii}", lii);
        await AddDeadLineItemCommand.ExecuteAsync(lii);
    }

    [RelayCommand]
    private async Task AddDeadLineItem(DeadLineItemInfo lii)
    {
        DeadLineItems.Add(lii);
        lii.PropertyChanged += (sender, _) => { _storage.UpdateDataAsync((DeadLineItemInfo)sender!); };
        if (!_isLoaded) return;
        if (lii.PrimaryKey is -1 || await _storage.FindDataAsync(lii.PrimaryKey) is null)
            await _storage.InsertDataAsync(lii);
        else
            await _storage.UpdateDataAsync(lii);
    }

    [RelayCommand]
    private async Task SaveDeadLineItems()
    {
        await _storage.UpdateDataAsync(DeadLineItems);
    }
}