using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUtility.Models;
using PropertyGenerator.Avalonia;
using ToDoList.Core.Abstract.Controls;
using ToDoList.Core.Abstract.ViewModels;
using ToDoList.Core.Abstract.Views;
using ToDoListDb.Abstract.Model;
using TaskStatus = ToDoListDb.Abstract.Model.TaskStatus;

namespace ToDoList.Core.Views;

public partial class MainMenuView : ViewModelUserControlBase<IMainMenuViewModel>, IMainMenuView
{
    public MainMenuView()
    {
        InitializeComponent();
        ListDisplay.ItemsSource = _displayItems;
    }

    private readonly ObservableCollection<BackLogViewItem> _displayItems = [];
}