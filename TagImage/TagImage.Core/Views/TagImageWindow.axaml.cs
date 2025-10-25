using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUtility.Services;

namespace TagImage.Core.Views;

public partial class TagImageWindow : Window,IStartupWindow
{
    public TagImageWindow()
    {
        InitializeComponent();
    }
}