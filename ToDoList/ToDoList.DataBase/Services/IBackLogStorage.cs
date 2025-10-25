namespace ToDoList.DataBase.Services;

public interface IBackLogStorage : IDependencyInjection
{
    Task<BackLog> CreateNewBackLogAsync(string title, string? description = null, BackGroup? taskGroup = null);
    Task<bool> DeleteBackLogAsync(int taskId);
    Task DeleteAllBackLogsAsync(BackGroup? taskGroup = null);
    Task UpdateBackLogAsync(BackLog newBackLog);
    Task<BackLog> GetBackLogAsync(int taskId);
    Task<IEnumerable<BackLog>> GetAllBackLogsAsync(BackGroup? group = null);
}