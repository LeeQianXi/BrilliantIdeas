namespace ToDoList.Core.Abstract.ViewModels;

public interface ISplashViewModel : IDependencyInjection, INotifyPropertyChanged
{
    void SplashCompleted();
    IAsyncEnumerator<YieldInstruction?> LoadGroupInfo();
    IAsyncEnumerator<YieldInstruction?> LoadTaskInfo();
    Task ApplyInitWith();
}