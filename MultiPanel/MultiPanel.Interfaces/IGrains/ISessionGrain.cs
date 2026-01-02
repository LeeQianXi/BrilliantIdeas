using Orleans;

namespace MultiPanel.Interfaces.IGrains;

[Alias("MultiPanel.Interfaces.IGrains.ISessionGrain")]
public interface ISessionGrain : IGrainWithGuidKey
{
    [Alias("BindToUser")]
    Task BindToUser(int userId);

    [Alias("VerifySession")]
    Task<bool> VerifySession(string accessToken);

    [Alias("IsSessionValid")]
    Task<bool> IsSessionValid();
}