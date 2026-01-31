namespace DLManager.Plugin.GadgetPlugin;

[DynamicLoading(nameof(GadgetPlugin))]
public class GadgetPlugin(IServiceProvider serviceProvider) : BasePlugin(serviceProvider)
{
    public override List<PluginViewEntry> Views { get; } =
    [
        CreateEntry(sp => new CountdownTimerView(), "倒计时器")
    ];
}