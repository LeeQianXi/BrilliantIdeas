using System.Drawing;
using DIAbstract.Services;
using ToDoListDb.Abstract.Model;

namespace ToDoListDb.Abstract;

public interface IBackGroupStorage : IDependencyInjection
{
    Task<BackGroup> CreateNewGroupAsync(string groupName, string color = "#FFFFFF");
    Task<bool> RemoveGroupAsync(BackGroup group);
    Task<bool> ContainsGroupAsync(string groupName);
    Task<bool> ContainsGroupAsync(int groupId);
    Task<BackGroup> GetGroupAsync(string groupName);
    Task<IEnumerable<string>> GetAllGroupNamesAsync();
    Task<IEnumerable<BackGroup>> GetAllGroupsAsync();
    Task<bool> ChangeGroupNameAsync(int groupId, string newGroupName);
    Task<bool> ChangeGroupColorAsync(int groupId, Color newColor);
    Task<bool> ChangeGroupColorAsync(int groupId, int newColor);
    Task<bool> ChangeGroupColorAsync(int groupId, string newColor);
}