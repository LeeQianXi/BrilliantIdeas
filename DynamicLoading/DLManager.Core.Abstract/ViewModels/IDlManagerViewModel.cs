using System.Collections.ObjectModel;
using DIAbstract;
using DLManager.Core.Abstract.Plugin;

namespace DLManager.Core.Abstract.ViewModels;

public interface IDlManagerViewModel : IDependencyInjection
{
    ReadOnlyObservableCollection<PluginViewInfo> PluginViewInfos { get; }
}