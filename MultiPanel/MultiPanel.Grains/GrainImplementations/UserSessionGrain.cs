using Microsoft.Extensions.Logging;
using MultiPanel.Abstractions.DTOs;
using MultiPanel.Grains.State;
using MultiPanel.Interfaces.IGrains;
using MultiPanel.Shared.Exceptions;
using MultiPanel.Shared.Services;
using Orleans;
using Orleans.Providers;

namespace MultiPanel.Grains.GrainImplementations;

[StorageProvider(ProviderName = "MySqlStore")]
public class UserSessionGrain(
    ILogger<UserSessionGrain> logger,
    IPasswordHasher passwordHasher,
    IGrainFactory grainFactory,
    ITokenGenerator tokenGenerator)
    : Grain<SessionState>, IUserSessionGrain
{
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;

    public async Task<SessionInfo> CreateSession(
        string userId,
        string username,
        DateTime expiry,
        DateTime refreshExpiry,
        string ipAddress,
        string userAgent,
        Dictionary<string, string>? claims = null)
    {
        if (State.IsActive)
            throw new AuthException("Session already exists");

        State.UserId = userId;
        State.Username = username;
        State.CreatedAt = DateTime.UtcNow;
        State.ExpiresAt = expiry;
        State.RefreshTokenExpiresAt = refreshExpiry;
        State.IpAddress = ipAddress;
        State.UserAgent = userAgent;
        State.IsActive = true;

        if (claims != null)
            State.Claims = claims;

        // 生成访问令牌和刷新令牌的哈希
        var accessToken = await GenerateAccessToken();
        var refreshToken = await GenerateRefreshToken();

        State.AccessTokenHash = passwordHasher.Hash(accessToken);
        State.RefreshTokenHash = passwordHasher.Hash(refreshToken);

        await WriteStateAsync();

        // 通知用户账户Grain添加此会话
        var userAccountGrain = grainFactory.GetGrain<IUserAccountGrain>(userId);
        await userAccountGrain.AddSession(State.SessionId);

        logger.LogInformation("Session {SessionId} created for user {UserId}",
            State.SessionId, State.UserId);

        return ToSessionInfo(accessToken, refreshToken);
    }

    public Task<SessionInfo> GetSessionInfo()
    {
        if (!State.IsActive)
            throw new AuthException("Session is not active");

        return Task.FromResult(ToSessionInfo());
    }

    public Task<bool> ValidateSession(string accessTokenHash)
    {
        if (!State.IsActive)
            return Task.FromResult(false);

        if (DateTime.UtcNow > State.ExpiresAt)
        {
            _ = InvalidateSession("Session expired");
            return Task.FromResult(false);
        }

        if (State.RevokedAt.HasValue)
            return Task.FromResult(false);

        return Task.FromResult(passwordHasher.Verify(accessTokenHash, State.AccessTokenHash));
    }

    public Task<bool> ValidateRefreshToken(string refreshTokenHash)
    {
        if (!State.IsActive)
            return Task.FromResult(false);

        if (DateTime.UtcNow > State.RefreshTokenExpiresAt)
        {
            _ = InvalidateSession("Refresh token expired");
            return Task.FromResult(false);
        }

        if (State.RevokedAt.HasValue)
            return Task.FromResult(false);

        return Task.FromResult(passwordHasher.Verify(refreshTokenHash, State.RefreshTokenHash));
    }

    public Task<string> GenerateAccessToken()
    {
        return Task.FromResult(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"));
    }

    public Task<string> GenerateRefreshToken()
    {
        return Task.FromResult(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"));
    }

    public async Task<TokenInfo> RefreshTokens(string refreshTokenHash)
    {
        if (!await ValidateRefreshToken(refreshTokenHash))
            throw new AuthException("Invalid refresh token");

        // 生成新令牌
        var newAccessToken = await GenerateAccessToken();
        var newRefreshToken = await GenerateRefreshToken();

        // 更新状态
        State.AccessTokenHash = passwordHasher.Hash(newAccessToken);
        State.RefreshTokenHash = passwordHasher.Hash(newRefreshToken);

        // 延长刷新令牌有效期（如果快过期了）
        if (State.RefreshTokenExpiresAt < DateTime.UtcNow.AddDays(1))
            State.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        State.ExpiresAt = DateTime.UtcNow.AddHours(2);
        await WriteStateAsync();

        logger.LogInformation("Session {SessionId} tokens refreshed", State.SessionId);

        return new TokenInfo
        {
            Token = newAccessToken,
            Type = "Bearer",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = State.ExpiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = State.RefreshTokenExpiresAt
        };
    }

    public async Task<bool> InvalidateSession(string reason = "User logged out")
    {
        if (!State.IsActive)
            return false;

        State.IsActive = false;
        State.RevokedAt = DateTime.UtcNow;
        State.RevokedReason = reason;
        await WriteStateAsync();

        // 通知用户账户Grain移除此会话
        try
        {
            var userAccountGrain = grainFactory.GetGrain<IUserAccountGrain>(State.UserId);
            await userAccountGrain.RemoveSession(State.SessionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove session {SessionId} from user {UserId}",
                State.SessionId, State.UserId);
        }

        logger.LogInformation("Session {SessionId} invalidated. Reason: {Reason}",
            State.SessionId, reason);

        return true;
    }

    public async Task<bool> ExtendSession(DateTime newExpiry)
    {
        if (!State.IsActive)
            return false;

        State.ExpiresAt = newExpiry;
        await WriteStateAsync();

        logger.LogDebug("Session {SessionId} extended to {NewExpiry}",
            State.SessionId, State.ExpiresAt);

        return true;
    }

    public async Task<bool> UpdateActivity()
    {
        if (!State.IsActive)
            return false;

        // 如果会话快过期了，自动延长
        if (State.ExpiresAt < DateTime.UtcNow.AddMinutes(5))
        {
            State.ExpiresAt = DateTime.UtcNow.AddHours(1);
            await WriteStateAsync();
        }

        return true;
    }

    public Task<bool> IsActive()
    {
        return Task.FromResult(State.IsActive &&
                               !State.RevokedAt.HasValue &&
                               DateTime.UtcNow < State.ExpiresAt);
    }

    public Task<bool> IsExpired()
    {
        return Task.FromResult(DateTime.UtcNow > State.ExpiresAt);
    }

    public async Task Cleanup()
    {
        if (!State.IsActive || DateTime.UtcNow > State.ExpiresAt.AddDays(1))
        {
            await ClearStateAsync();
            logger.LogDebug("Session {SessionId} cleaned up", State.SessionId);
        }
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        State.SessionId = this.GetPrimaryKey();
        return base.OnActivateAsync(cancellationToken);
    }

    private SessionInfo ToSessionInfo(string? accessToken = null, string? refreshToken = null)
    {
        return new SessionInfo
        {
            SessionId = State.SessionId,
            UserId = State.UserId,
            Username = State.Username,
            CreatedAt = State.CreatedAt,
            ExpiresAt = State.ExpiresAt,
            AccessToken = accessToken ?? "[REDACTED]",
            RefreshToken = refreshToken ?? "[REDACTED]",
            RefreshTokenExpiresAt = State.RefreshTokenExpiresAt,
            Claims = State.Claims,
            IpAddress = State.IpAddress,
            UserAgent = State.UserAgent,
            IsActive = State.IsActive
        };
    }
}