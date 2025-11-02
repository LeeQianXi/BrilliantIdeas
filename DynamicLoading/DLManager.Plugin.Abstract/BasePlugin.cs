namespace DLManager.Plugin.Abstract;

public abstract class BasePlugin
{
    public abstract IEnumerable<PluginViewEntry> Views { get; }
}