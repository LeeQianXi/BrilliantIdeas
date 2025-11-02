namespace DLManager.Core.ViewModels;

public class DlManagerViewModel(IServiceProvider serviceProvider) : ViewModelBase, IDlManagerViewModel
{
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<DlManagerViewModel>>();

    public AvaloniaList<PluginViewInfo> PluginViews { get; } =
        new(from pair in ServiceLocator.Plugins
            from knt in pair.Value
            select new PluginViewInfo(pair.Key, knt.Key, knt.Value.Item1));
}