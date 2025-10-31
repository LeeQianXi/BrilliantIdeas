namespace DLManager.Plugin.Abstract;

[AttributeUsage(AttributeTargets.Class)]
public class DynamicLoadingAttribute(string pluginId) : Attribute
{
    public string PluginId { get; } = pluginId;
}

[AttributeUsage(AttributeTargets.Class)]
public class DeclareViewAttribute(string viewName, PluginLifeCycle lifeCycle = PluginLifeCycle.Transient) : Attribute
{
    public string ViewName { get; } = viewName;
    public PluginLifeCycle LifeCycle { get; } = lifeCycle;
}