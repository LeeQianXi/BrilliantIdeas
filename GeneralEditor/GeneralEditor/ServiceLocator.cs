using GeneralEditor.Core.Abstract.View;
using GeneralEditor.Core.Abstract.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralEditor;

public class ServiceLocator
{
    public static IServiceProvider ServiceProvider { get; internal set; } = null!;

    public ITechNodesEditorView TechNodesEditorView =>
        ServiceProvider.GetRequiredService<ITechNodesEditorView>();

    public IGeneralEditorMenuViewModel GeneralEditorMenuViewModel =>
        ServiceProvider.GetRequiredService<IGeneralEditorMenuViewModel>();
}