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
public class TokenGrain(
    ILogger<TokenGrain> logger,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator)
    : Grain<TokenState>, ITokenGrain
{
    public async Task<bool> ValidateToken(string token, bool validateExpiry = true)
    {
        if (State.IsRevoked || State.IsUsed)
            return false;

        if (validateExpiry && DateTime.UtcNow > State.ExpiresAt)
        {
            await MarkAsExpired();
            return false;
        }

        return passwordHasher.Verify(token, State.TokenHash);
    }

    public async Task<TokenValidationResult> ValidateTokenWithDetails(string token)
    {
        if (State.IsRevoked)
            return TokenValidationResult.Invalid("Token revoked");

        if (State.IsUsed)
            return TokenValidationResult.Invalid("Token already used");

        if (DateTime.UtcNow > State.ExpiresAt)
        {
            await MarkAsExpired();
            return TokenValidationResult.Invalid("Token expired");
        }

        if (!passwordHasher.Verify(token, State.TokenHash))
            return TokenValidationResult.Invalid("Invalid token");

        return TokenValidationResult.Valid(State.UserId, State.SessionId, State.ExpiresAt, State.Scopes);
    }

    public Task<bool> IsTokenValid()
    {
        return Task.FromResult(!State.IsRevoked &&
                               !State.IsUsed &&
                               DateTime.UtcNow < State.ExpiresAt);
    }

    public async Task<string> GenerateToken(
        string userId,
        Guid sessionId,
        DateTime expiry,
        string tokenType,
        List<string>? scopes = null)
    {
        if (!string.IsNullOrEmpty(State.TokenHash))
            throw new AuthException("Token already generated");

        var token = tokenGenerator.GenerateToken(userId, sessionId, expiry, tokenType, scopes);

        State.UserId = userId;
        State.SessionId = sessionId;
        State.TokenHash = passwordHasher.Hash(token);
        State.CreatedAt = DateTime.UtcNow;
        State.ExpiresAt = expiry;
        State.TokenType = tokenType;
        State.Scopes = scopes ?? new List<string>();
        State.IsRevoked = false;
        State.IsUsed = false;

        await WriteStateAsync();

        logger.LogDebug("Token {TokenId} generated for user {UserId}",
            State.TokenId, State.UserId);

        return token;
    }

    public async Task<bool> RevokeToken(string reason = "Revoked by system")
    {
        if (State.IsRevoked)
            return false;

        State.IsRevoked = true;
        State.RevokedAt = DateTime.UtcNow;
        await WriteStateAsync();

        logger.LogInformation("Token {TokenId} revoked. Reason: {Reason}",
            State.TokenId, reason);

        return true;
    }

    public async Task<bool> MarkAsUsed()
    {
        if (State.IsUsed)
            return false;

        State.IsUsed = true;
        await WriteStateAsync();

        return true;
    }

    public Task<TokenInfo> GetTokenInfo()
    {
        return Task.FromResult(new TokenInfo
        {
            Token = "[HIDDEN]",
            Type = State.TokenType,
            IssuedAt = State.CreatedAt,
            ExpiresAt = State.ExpiresAt,
            RefreshToken = string.Empty,
            RefreshTokenExpiresAt = DateTime.MinValue
        });
    }

    public Task<string> GetUserId()
    {
        return Task.FromResult(State.UserId);
    }

    public Task<Guid> GetSessionId()
    {
        return Task.FromResult(State.SessionId);
    }

    public async Task<bool> CleanupIfExpired()
    {
        if (State.IsRevoked || DateTime.UtcNow > State.ExpiresAt.AddDays(1))
        {
            await ClearStateAsync();
            logger.LogDebug("Token {TokenId} cleaned up", State.TokenId);
            return true;
        }

        return false;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        State.TokenId = this.GetPrimaryKeyString();
        return base.OnActivateAsync(cancellationToken);
    }

    private async Task MarkAsExpired()
    {
        State.IsRevoked = true;
        State.RevokedAt = DateTime.UtcNow;
        await WriteStateAsync();
    }
}