using DIAbstract;

namespace ToDoList.DataBase;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseToDoListDbCore()
        {
            return collection
                .AddSingleton<BackLogStorage>()
                .AddSingleton<IAsyncLifecycle, BackLogStorage>(s => s.GetRequiredService<BackLogStorage>())
                .AddSingleton<IBackLogStorage, BackLogStorage>(s => s.GetRequiredService<BackLogStorage>())
                .AddSingleton<BackGroupStorage>()
                .AddSingleton<IAsyncLifecycle, BackGroupStorage>(s => s.GetRequiredService<BackGroupStorage>())
                .AddSingleton<IBackGroupStorage, BackGroupStorage>(s => s.GetRequiredService<BackGroupStorage>());
        }
    }
}