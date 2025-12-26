using MultiPanel.Abstractions.DTOs;
using MultiPanel.Interfaces.IGrains;
using Orleans;

namespace MultiPanel.Grains.GrainImplementations;

public class AccountGrain : Grain, IAccountGrain
{
    public async Task<bool> Exist()
    {
        throw new NotImplementedException();
    }

    public async Task<AccountInfo> TryLogin(string passwordHash)
    {
        throw new NotImplementedException();
    }

    public async Task<AccountInfo> TryRegister(string passwordHash)
    {
        throw new NotImplementedException();
    }

    public async Task TryDeleteAccount(string token)
    {
        throw new NotImplementedException();
    }

    public async Task DeactivateAccount()
    {
        throw new NotImplementedException();
    }

    public async Task ActivateAccount()
    {
        throw new NotImplementedException();
    }
}