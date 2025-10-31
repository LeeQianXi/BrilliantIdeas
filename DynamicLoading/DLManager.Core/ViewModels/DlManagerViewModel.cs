using Avalonia.Collections;

namespace DLManager.Core.ViewModels;

public class DlManagerViewModel(IServiceProvider serviceProvider) : ViewModelBase, IDlManagerViewModel
{
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<DlManagerViewModel>>();

    public AvaloniaList<string> Plugs { get; } = new(ServiceLocator.Plugins.Keys);
}