using MultiPanel.Abstractions.DTOs;
using Orleans;

namespace MultiPanel.Interfaces.IGrains;

/// 会话管理Grain
[Alias("MultiPanel.Interfaces.IGrains.IUserSessionGrain")]
public interface IUserSessionGrain : IGrainWithGuidKey
{
    // 会话管理
    [Alias("CreateSession")]
    Task<SessionInfo> CreateSession(
        string userId,
        string username,
        DateTime expiry,
        DateTime refreshExpiry,
        string ipAddress,
        string userAgent,
        Dictionary<string, string>? claims = null);

    [Alias("GetSessionInfo")]
    Task<SessionInfo> GetSessionInfo();

    [Alias("ValidateSession")]
    Task<bool> ValidateSession(string accessTokenHash);

    [Alias("ValidateRefreshToken")]
    Task<bool> ValidateRefreshToken(string refreshTokenHash);

    // 令牌管理
    [Alias("GenerateAccessToken")]
    Task<string> GenerateAccessToken();

    [Alias("GenerateRefreshToken")]
    Task<string> GenerateRefreshToken();

    [Alias("RefreshTokens")]
    Task<TokenInfo> RefreshTokens(string refreshTokenHash);

    // 会话操作
    [Alias("InvalidateSession")]
    Task<bool> InvalidateSession(string reason = "User logged out");

    [Alias("ExtendSession")]
    Task<bool> ExtendSession(DateTime newExpiry);

    [Alias("UpdateActivity")]
    Task<bool> UpdateActivity();

    // 验证
    [Alias("IsActive")]
    Task<bool> IsActive();

    [Alias("IsExpired")]
    Task<bool> IsExpired();

    // 清理
    [Alias("Cleanup")]
    Task Cleanup();
}