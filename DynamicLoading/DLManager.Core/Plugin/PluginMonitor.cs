using DLManager.Core.Abstract.Plugin;
using Microsoft.Extensions.Hosting;

namespace DLManager.Core.Plugin;

public class PluginMonitor(ILogger<PluginMonitor> logger, IServiceProvider serviceProvider, IPluginContainer container)
    : IHostedService
{
    private static readonly FileSystemWatcher FileSystemWatcher;

    static PluginMonitor()
    {
        if (!Directory.Exists(PluginPath))
            Directory.CreateDirectory(PluginPath);
        FileSystemWatcher = new FileSystemWatcher(PluginPath, "*.dll")
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName |
                           NotifyFilters.CreationTime |
                           NotifyFilters.LastWrite,
            EnableRaisingEvents = false
        };
    }

    private static string ProgramPath => Path.GetFullPath(Path.GetDirectoryName(Environment.ProcessPath!)!);
    private static string PluginPath => Path.Combine(ProgramPath, "Plugins");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        FileSystemWatcher.Created += OnCreated;
        FileSystemWatcher.Deleted += OnDeleted;
        FileSystemWatcher.Renamed += OnRenamed;
        LoadExistingFiles();
        FileSystemWatcher.EnableRaisingEvents = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        FileSystemWatcher.EnableRaisingEvents = false;
        FileSystemWatcher.Created -= OnCreated;
        FileSystemWatcher.Deleted -= OnDeleted;
        FileSystemWatcher.Renamed -= OnRenamed;
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        RenamePlugin(e.OldFullPath, e.FullPath);
        logger.LogInformation("Renamed Plugin {OldPath} to {NewPath}", e.OldFullPath, e.FullPath);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        DeletePlugin(e.FullPath);
        logger.LogInformation("Deleted a Plugin {PluginName}", e.Name);
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (File.Exists(e.FullPath))
        {
            ParsePluginData(e.FullPath);
            logger.LogInformation("Find a new Plugin {PluginName}", e.Name);
        }
    }

    private void LoadExistingFiles()
    {
        if (!Directory.Exists(PluginPath)) return;

        var files = Directory.GetFiles(PluginPath, "*.dll", SearchOption.TopDirectoryOnly);
        logger.LogInformation("Found {PluginCount} Plugins", files.Length);
        foreach (var file in files) ParsePluginData(file);
    }

    private void RenamePlugin(string oldPath, string newPath)
    {
        if (File.Exists(newPath))
            container.RenamePlugin(oldPath, newPath);
    }

    private void DeletePlugin(string pluginPath)
    {
        container.RemovePlugin(pluginPath);
    }

    private void ParsePluginData(string pluginPath)
    {
        try
        {
            logger.LogInformation("Try Loading Plugin {PluginPath}", pluginPath);
            var ass = Assembly.LoadFile(pluginPath);
            foreach (var type in ass.GetTypes()
                         .Where(t => t.GetCustomAttribute<DynamicLoadingAttribute>() is not null)
                         .Where(t => t.IsAssignableTo(typeof(BasePlugin)))
                         .Where(t => !t.IsAbstract))
            {
                var pluginId = type.GetCustomAttribute<DynamicLoadingAttribute>()!.PluginId;
                var pluginInstance = (BasePlugin)Activator.CreateInstance(type, serviceProvider)!;
                container.AddPlugin(new PluginInfo(pluginPath, pluginId, pluginInstance));
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to Load Plugin {PluginPath}", pluginPath);
        }
    }
}