namespace DLManager.Core.Abstract.ViewModels;

public interface IDlManagerViewModel : IDependencyInjection
{
    AvaloniaList<PluginViewInfo> PluginViews { get; }
}

public record PluginViewInfo(string PluginId, string ViewId, string DisplayName);