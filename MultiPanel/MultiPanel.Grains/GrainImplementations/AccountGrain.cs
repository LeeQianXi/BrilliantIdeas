using MultiPanel.Abstractions.DTOs;
using MultiPanel.Abstractions.IRepository;
using MultiPanel.Interfaces.IGrains;

namespace MultiPanel.Grains.GrainImplementations;

[CollectionAgeLimit(Minutes = 15)]
public class AccountGrain(IAccountRepository accountRepository) : Grain, IAccountGrain
{
    private string Username => this.GetPrimaryKeyString();

    public async Task<bool> ExistAsync()
    {
        return await accountRepository.ExistUserAsync(Username);
    }

    public async Task<AuthDto> RegisterAsync(string password)
    {
        if (await accountRepository.ExistUserAsync(Username))
            throw new InvalidOperationException("UserName already exists");

        var uid = await accountRepository.InsertUserAsync(Username, password);
        var rid = await accountRepository.GetRoleByNameAsync("User");
        if (rid is not null) await accountRepository.AssignRoleToUserAsync(uid, rid.RoleId);
        return await accountRepository.GenerateTokenAsync(uid);
    }

    public async Task<AuthDto> LoginAsync(string password)
    {
        if (!await accountRepository.ExistUserAsync(Username))
            throw new InvalidOperationException("User does not exist");

        var uid = await accountRepository.CheckPasswordAsync(Username, password);
        if (uid is -1) throw new InvalidOperationException("Invalid password");

        return await accountRepository.GenerateTokenAsync(uid);
    }

    public async Task<AuthDto> RefreshAsync(AuthDto dto)
    {
        var newDto = await accountRepository.UpdateAccessTokenAsync(dto.RefreshToken, dto.UserId);
        if (newDto is null) throw new InvalidOperationException("Invalid Token");
        return newDto;
    }

    public async Task<bool> DeleteAccountAsync(string password)
    {
        if (!await accountRepository.ExistUserAsync(Username))
            return false;
        return await accountRepository.DeleteUserAsync(Username, password);
    }
}