using Microsoft.Extensions.DependencyInjection;

namespace DLManager.Plugin.Abstract;

[AttributeUsage(AttributeTargets.Class)]
public class DynamicLoadingAttribute(string pluginId) : Attribute
{
    public string PluginId { get; } = pluginId;
}

[AttributeUsage(AttributeTargets.Class)]
public class DeclareViewAttribute : Attribute
{
    public DeclareViewAttribute(string viewName, ServiceLifetime lifeCycle = ServiceLifetime.Transient)
    {
        ViewName = viewName;
        LifeCycle = lifeCycle;
        if (lifeCycle is ServiceLifetime.Scoped)
            throw new Exception("LifeCycle can't be Scoped");
    }

    public string ViewName { get; }
    public ServiceLifetime LifeCycle { get; }
}