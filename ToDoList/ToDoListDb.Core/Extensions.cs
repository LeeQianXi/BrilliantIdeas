using ToDoListDb.Abstract;
using ToDoListDb.Core.Services;

namespace ToDoListDb.Core;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseToDoListDbCore()
        {
            return collection
                .AddSingleton<IBackLogStorage, BackLogStorage>()
                .AddSingleton<IBackGroupStorage, BackGroupStorage>();
        }
    }
}