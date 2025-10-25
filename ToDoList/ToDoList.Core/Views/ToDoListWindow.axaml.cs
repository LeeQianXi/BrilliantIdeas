using Avalonia.Controls;
using AvaloniaUtility.Services;

namespace ToDoList.Core.Views;

public partial class ToDoListWindow : Window, IStartupWindow
{
    public ToDoListWindow()
    {
        InitializeComponent();
    }
}