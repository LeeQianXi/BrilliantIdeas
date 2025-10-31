using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DLManager.Plugin.Abstract.Views;

namespace DLManager.Plugin.Test.Views;

public partial class TestView : UserControl, IPluginView
{
    public static TestView CreateInstance() => new TestView();

    public TestView()
    {
        InitializeComponent();
    }
}