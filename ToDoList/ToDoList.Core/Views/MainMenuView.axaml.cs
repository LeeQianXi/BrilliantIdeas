using System.Collections.ObjectModel;
using AvaloniaUtility.Views;
using ToDoList.Core.Abstract.Bases;
using ToDoList.Core.Abstract.Controls;
using ToDoList.Core.Abstract.ViewModels;
using ToDoList.Core.Abstract.Views;

namespace ToDoList.Core.Views;

public partial class MainMenuView : ViewModelUserControlBase<IMainMenuViewModel>, IMainMenuView
{
    public MainMenuView()
    {
        InitializeComponent();
        //ListDisplay.ItemsSource = _displayItems;
    }

    private readonly ObservableCollection<BackLogViewItem> _displayItems = [];
}