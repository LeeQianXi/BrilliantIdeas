using DIAbstract;

namespace DeadLine.DataBase.Core;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseDeadLineDataBase()
        {
            return collection
                .AddSingleton<DeadLineInfoStorage>()
                .AddSingleton<IAsyncLifecycle, DeadLineInfoStorage>(s => s.GetRequiredService<DeadLineInfoStorage>())
                .AddSingleton<IDeadLineInfoStorage, DeadLineInfoStorage>(s =>
                    s.GetRequiredService<DeadLineInfoStorage>());
        }
    }
}