using MultiPanel.Abstractions.DTOs;
using Orleans;

namespace MultiPanel.Interfaces.IGrains;

[Alias("MultiPanel.Interfaces.IGrains.IAuthenticationAggregateGrain")]
public interface IAuthenticationAggregateGrain : IGrainWithGuidKey
{
    // 完整认证流程
    [Alias("Register")]
    Task<AuthResponse> Register(UserRegistration registration, string ipAddress, string userAgent);

    [Alias("Login")]
    Task<AuthResponse> Login(LoginRequest request);

    [Alias("Logout")]
    Task<AuthResponse> Logout(Guid sessionId, string accessToken);

    [Alias("RefreshTokens")]
    Task<AuthResponse> RefreshTokens(Guid sessionId, string refreshToken);

    // 会话验证
    [Alias("ValidateSession")]
    Task<AuthResponse> ValidateSession(ValidateSessionRequest request);

    // 批量操作
    [Alias("GetUserSessions")]
    Task<List<SessionInfo>> GetUserSessions(string userId);

    [Alias("RevokeAllUserSessions")]
    Task<bool> RevokeAllUserSessions(string userId, string reason);

    // 管理员操作
    [Alias("ForceLogout")]
    Task<bool> ForceLogout(Guid sessionId, string reason);

    [Alias("DeactivateUser")]
    Task<bool> DeactivateUser(string userId, string reason);
}