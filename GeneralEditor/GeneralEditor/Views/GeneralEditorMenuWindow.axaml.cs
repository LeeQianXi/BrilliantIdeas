using Avalonia.Controls;
using AvaloniaUtility.Services;
using GeneralEditor.Database.Abstract.Services;

namespace GeneralEditor.Views;

public partial class GeneralEditorMenuWindow : Window, IStartupWindow
{
    public GeneralEditorMenuWindow(IGeneralRepository repository)
    {
        InitializeComponent();
    }
}