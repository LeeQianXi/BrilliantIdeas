using MultiPanel.Abstractions.DTOs;
using Orleans;

namespace MultiPanel.Interfaces.IGrains;

public interface IAccountGrain : IGrainWithStringKey
{
    Task<bool> Exist();
    Task<AccountInfo> TryLogin(string passwordHash);
    Task<AccountInfo> TryRegister(string passwordHash);
    Task TryDeleteAccount(string token);
    Task DeactivateAccount();
    Task ActivateAccount();
}