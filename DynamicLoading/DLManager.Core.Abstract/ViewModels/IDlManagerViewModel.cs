using Avalonia.Collections;

namespace DLManager.Core.Abstract.ViewModels;

public interface IDlManagerViewModel:IDependencyInjection
{
    AvaloniaList<string> Plugs { get; } 
}