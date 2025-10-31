using DLManager.Plugin.Abstract.Views;
using Microsoft.Extensions.DependencyInjection;
using NetUtility;

namespace DLManager.Plugin.Abstract;

public abstract class BasePlugin
{
    public abstract IDictionary<string, SupplierFactory<IPluginView>> Views { get; }
    public abstract IList<Uri>AvaloniaStyles { get; }
}