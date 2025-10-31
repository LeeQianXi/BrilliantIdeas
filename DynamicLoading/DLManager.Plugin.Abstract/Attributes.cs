using System.Reflection;

namespace DLManager.Plugin.Abstract;

[AttributeUsage(AttributeTargets.Class)]
public class DynamicLoadingAttribute(string pluginId) : Attribute
{
    public string PluginId { get; } = pluginId;
}