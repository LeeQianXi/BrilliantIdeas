namespace DeadLine.Core;

public partial class AppCommand : ReactiveObject
{
    private static ServiceLocator ServiceLocator => ServiceLocator.Instance;

    [RelayCommand]
    private void CloseApp()
    {
        ServiceLocator.DeadLineWindow.Close();
    }

    [RelayCommand]
    private void NewTask()
    {
        ServiceLocator.DeadLineViewModel.NewDeadLineItemCommand.Execute(null);
    }

    [RelayCommand]
    private void HideWindow()
    {
        ServiceLocator.DeadLineWindow.Hide();
    }

    [RelayCommand]
    private void ShowWindow()
    {
        ServiceLocator.DeadLineWindow.Show();
    }
}