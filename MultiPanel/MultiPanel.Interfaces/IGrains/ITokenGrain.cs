using MultiPanel.Abstractions.DTOs;
using Orleans;

namespace MultiPanel.Interfaces.IGrains;

/// 令牌管理Grain
[Alias("MultiPanel.Interfaces.IGrains.ITokenGrain")]
public interface ITokenGrain : IGrainWithStringKey
{
    // 令牌验证
    [Alias("ValidateToken")]
    Task<bool> ValidateToken(string token, bool validateExpiry = true);

    [Alias("ValidateTokenWithDetails")]
    Task<TokenValidationResult> ValidateTokenWithDetails(string token);

    [Alias("IsTokenValid")]
    Task<bool> IsTokenValid();

    // 令牌管理
    [Alias("GenerateToken")]
    Task<string> GenerateToken(string userId, Guid sessionId, DateTime expiry, string tokenType,
        List<string>? scopes = null);

    [Alias("RevokeToken")]
    Task<bool> RevokeToken(string reason = "Revoked by system");

    [Alias("MarkAsUsed")]
    Task<bool> MarkAsUsed();

    // 令牌信息
    [Alias("GetTokenInfo")]
    Task<TokenInfo> GetTokenInfo();

    [Alias("GetUserId")]
    Task<string> GetUserId();

    [Alias("GetSessionId")]
    Task<Guid> GetSessionId();

    // 清理
    [Alias("CleanupIfExpired")]
    Task<bool> CleanupIfExpired();
}

[GenerateSerializer]
[Alias("MultiPanel.Interfaces.IGrains.TokenValidationResult")]
public class TokenValidationResult
{
    [Id(0)] public bool IsValid { get; set; }

    [Id(1)] public string? Error { get; set; }

    [Id(2)] public string UserId { get; set; } = string.Empty;

    [Id(3)] public Guid SessionId { get; set; }

    [Id(4)] public DateTime ExpiresAt { get; set; }

    [Id(5)] public List<string> Scopes { get; set; } = new();

    public static TokenValidationResult Valid(string userId, Guid sessionId, DateTime expiresAt, List<string> scopes)
    {
        return new TokenValidationResult
        {
            IsValid = true,
            UserId = userId,
            SessionId = sessionId,
            ExpiresAt = expiresAt,
            Scopes = scopes
        };
    }

    public static TokenValidationResult Invalid(string error)
    {
        return new TokenValidationResult { IsValid = false, Error = error };
    }
}