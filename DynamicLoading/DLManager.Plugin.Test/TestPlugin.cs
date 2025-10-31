using DLManager.Plugin.Abstract;
using DLManager.Plugin.Abstract.Views;
using DLManager.Plugin.Test.Views;
using NetUtility;

namespace DLManager.Plugin.Test;

[DynamicLoading(nameof(TestPlugin))]
public class TestPlugin : BasePlugin
{
    public override Dictionary<string, SupplierFactory<IPluginView>> Views { get; } = new()
    {
        [nameof(TestView)] = TestView.CreateInstance
    };

    public override List<Uri> AvaloniaStyles { get; } =
        [new Uri("avares://DLManager.Plugin.Test/Styles/TestPluginStyles.axaml")];
}