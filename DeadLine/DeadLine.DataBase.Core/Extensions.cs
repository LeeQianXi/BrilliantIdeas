namespace DeadLine.DataBase.Core;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseDeadLineDataBase()
        {
            return collection
                .AddSingleton<IDeadLineInfoStorage, DeadLineInfoStorage>();
        }
    }
}