using System.ComponentModel;
using ToDoListDb.Abstract;
using ToDoListDb.Abstract.Model;

namespace ToDoList.Core.Abstract.ViewModels;

public interface ISplashViewModel : IDependencyInjection, INotifyPropertyChanged
{
    void SplashCompleted();
    IEnumerator<YieldInstruction?> LoadGroupInfo();
    IEnumerator<YieldInstruction?> LoadTaskInfo();
    Task ApplyInitWith();
}