using MultiPanel.Abstractions.DTOs;
using MultiPanel.Abstractions.IRepository;
using MultiPanel.Interfaces.IGrains;

namespace MultiPanel.Grains.GrainImplementations;

public class AccountGrain(IAccountRepository accountRepository) : Grain, IAccountGrain
{
    private string Username => this.GetPrimaryKeyString();

    public async Task<bool> ExistAsync()
    {
        return await accountRepository.ExistUserAsync(Username);
    }

    public async Task<AuthDto> RegisterAsync(string passwordHash)
    {
        if (await accountRepository.ExistUserAsync(Username))
            throw new InvalidOperationException("User already exists");

        var uid = await accountRepository.InsertUserAsync(Username, passwordHash);
        var rid = await accountRepository.GetRoleByNameAsync("User");
        if (rid is not null) await accountRepository.AssignRoleToUserAsync(uid, rid.RoleId);
        return await accountRepository.GenerateTokenAsync(uid);
    }

    public async Task<AuthDto> LoginAsync(string passwordHash)
    {
        if (!await accountRepository.ExistUserAsync(Username))
            throw new InvalidOperationException("User does not exist");

        var uid = await accountRepository.InsertUserAsync(Username, passwordHash);
        if (uid is -1) throw new InvalidOperationException("Invalid password");

        return await accountRepository.GenerateTokenAsync(uid);
    }

    public async Task<AuthDto> RefreshAsync(AuthDto dto)
    {
        if (!dto.IsValid) throw new InvalidOperationException("Invalid dto");

        var newAccessToken = await accountRepository.UpdateAccessTokenAsync(dto.RefreshToken, dto.UserId);
        dto.AccessToken = newAccessToken;
        return dto;
    }

    public async Task<bool> DeleteAccountAsync(string passwordHash)
    {
        if (!await accountRepository.ExistUserAsync(Username))
            throw new InvalidOperationException("User does not exist");

        return await accountRepository.DeleteUserAsync(Username, passwordHash);
    }
}