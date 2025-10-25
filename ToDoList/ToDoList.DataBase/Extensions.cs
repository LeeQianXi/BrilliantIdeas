namespace ToDoList.DataBase;

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