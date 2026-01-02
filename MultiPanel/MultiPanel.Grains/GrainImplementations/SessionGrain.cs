using MultiPanel.Abstractions.IRepository;
using MultiPanel.Grains.State;
using MultiPanel.Interfaces.IGrains;

namespace MultiPanel.Grains.GrainImplementations;

public class SessionGrain(IAccountRepository accountRepository) : Grain<SessionState>, ISessionGrain
{
    private Guid GrainKey => this.GetPrimaryKey();

    public async Task BindToUser(int userId)
    {
        State.Reset(userId);
        await WriteStateAsync();
    }

    public async Task<bool> VerifySession(string accessToken)
    {
        if (!await accountRepository.VerifyAccessTokenAsync(accessToken, State.UserId))
            return false;
        State.AccessToken = accessToken;
        await WriteStateAsync();
        return true;
    }

    public async Task<bool> IsSessionValid()
    {
        if (!State.IsValid())
            return false;
        return await accountRepository.VerifyAccessTokenAsync(State.AccessToken, State.UserId);
    }
}