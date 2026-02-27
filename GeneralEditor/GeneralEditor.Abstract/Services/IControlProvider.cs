using Avalonia.Controls;

namespace GeneralEditor.Core.Abstract.Services;

public interface IControlProvider
{
    string Title { get; }

    Control GetControl(IServiceProvider serviceProvider);
}