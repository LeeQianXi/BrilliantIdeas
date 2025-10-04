using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ToDoList.Core.Abstract.ViewModels;
using ToDoListCore.Abstract.Services;
using ToDoListCore.ViewModels;

namespace ToDoList.Core.Views;

public partial class ToDoListWindow : Window, IStartupWindow
{
    public ToDoListWindow()
    {
        InitializeComponent();
    }
}