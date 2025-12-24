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
public class UserAccountGrain(
    ILogger<UserAccountGrain> logger,
    IPasswordHasher passwordHasher,
    IGrainFactory grainFactory)
    : Grain<UserAccountState>, IUserAccountGrain
{
    public Task<UserInfo> GetUserInfo()
    {
        return Task.FromResult(new UserInfo
        {
            UserId = State.UserId,
            Username = State.Username,
            Email = State.Email,
            DisplayName = State.DisplayName,
            Roles = State.Roles,
            Claims = State.Claims,
            CreatedAt = State.CreatedAt,
            LastLogin = State.LastLogin,
            IsActive = State.IsActive,
            EmailVerified = State.EmailVerified
        });
    }

    public Task<bool> Exists()
    {
        return Task.FromResult(!string.IsNullOrEmpty(State.UserId));
    }

    public Task<List<Guid>> GetActiveSessions()
    {
        return Task.FromResult(State.ActiveSessions);
    }

    public async Task<bool> ValidatePassword(string password)
    {
        if (!State.IsActive)
            throw new AuthException("Account is deactivated");

        if (State.LockoutEnd.HasValue && State.LockoutEnd.Value > DateTime.UtcNow)
            throw new AuthException($"Account is locked until {State.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss}");

        var isValid = passwordHasher.Verify(password, State.PasswordHash);

        if (!isValid)
        {
            State.FailedLoginAttempts++;
            State.UpdatedAt = DateTime.UtcNow;

            // 锁定账户（5次失败尝试后锁定15分钟）
            if (State.FailedLoginAttempts >= 5)
            {
                State.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                await WriteStateAsync();
                throw new AuthException("Account locked due to too many failed attempts");
            }

            await WriteStateAsync();
            return false;
        }

        // 重置失败计数
        State.FailedLoginAttempts = 0;
        State.LockoutEnd = null;
        await WriteStateAsync();

        return true;
    }

    public Task<bool> IsLockedOut()
    {
        return Task.FromResult(State.LockoutEnd.HasValue && State.LockoutEnd.Value > DateTime.UtcNow);
    }

    public async Task ResetFailedLoginAttempts()
    {
        State.FailedLoginAttempts = 0;
        State.LockoutEnd = null;
        State.UpdatedAt = DateTime.UtcNow;
        await WriteStateAsync();
    }

    public async Task IncrementFailedLoginAttempts()
    {
        State.FailedLoginAttempts++;
        State.UpdatedAt = DateTime.UtcNow;

        if (State.FailedLoginAttempts >= 5) State.LockoutEnd = DateTime.UtcNow.AddMinutes(15);

        await WriteStateAsync();
    }

    public async Task UpdateLastLogin()
    {
        State.LastLogin = DateTime.UtcNow;
        State.UpdatedAt = DateTime.UtcNow;
        await WriteStateAsync();

        logger.LogInformation("User {UserId} logged in at {LoginTime}",
            State.UserId, State.LastLogin);
    }

    public async Task UpdateProfile(string? displayName, string? email)
    {
        if (!string.IsNullOrEmpty(displayName))
            State.DisplayName = displayName;

        if (!string.IsNullOrEmpty(email))
            State.Email = email;

        State.UpdatedAt = DateTime.UtcNow;
        await WriteStateAsync();
    }

    public async Task ChangePassword(string oldPassword, string newPassword)
    {
        if (!passwordHasher.Verify(oldPassword, State.PasswordHash))
            throw new AuthException("Current password is incorrect");

        State.PasswordHash = passwordHasher.Hash(newPassword);
        State.UpdatedAt = DateTime.UtcNow;

        // 使所有活动会话失效
        foreach (var sessionId in State.ActiveSessions)
            try
            {
                var sessionGrain = grainFactory.GetGrain<IUserSessionGrain>(sessionId);
                await sessionGrain.InvalidateSession("Password changed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate session {SessionId} for user {UserId}",
                    sessionId, State.UserId);
            }

        State.ActiveSessions.Clear();
        await WriteStateAsync();

        logger.LogInformation("User {UserId} changed password", State.UserId);
    }

    public async Task SetPasswordHash(string passwordHash)
    {
        State.PasswordHash = passwordHash;
        State.UpdatedAt = DateTime.UtcNow;
        await WriteStateAsync();
    }

    public async Task AddSession(Guid sessionId)
    {
        if (!State.ActiveSessions.Contains(sessionId))
        {
            State.ActiveSessions.Add(sessionId);
            State.UpdatedAt = DateTime.UtcNow;
            await WriteStateAsync();
        }
    }

    public async Task RemoveSession(Guid sessionId)
    {
        if (State.ActiveSessions.Remove(sessionId))
        {
            State.UpdatedAt = DateTime.UtcNow;
            await WriteStateAsync();
        }
    }

    public async Task Activate()
    {
        State.IsActive = true;
        State.UpdatedAt = DateTime.UtcNow;
        await WriteStateAsync();
    }

    public async Task Deactivate(string reason)
    {
        State.IsActive = false;
        State.UpdatedAt = DateTime.UtcNow;

        // 使所有活动会话失效
        foreach (var sessionId in State.ActiveSessions)
            try
            {
                var sessionGrain = grainFactory.GetGrain<IUserSessionGrain>(sessionId);
                await sessionGrain.InvalidateSession($"Account deactivated: {reason}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate session {SessionId} for user {UserId}",
                    sessionId, State.UserId);
            }

        State.ActiveSessions.Clear();
        await WriteStateAsync();

        logger.LogInformation("User {UserId} deactivated. Reason: {Reason}", State.UserId, reason);
    }

    public async Task<bool> ToggleEmailVerified(bool verified)
    {
        State.EmailVerified = verified;
        State.UpdatedAt = DateTime.UtcNow;
        await WriteStateAsync();
        return State.EmailVerified;
    }

    public async Task AddRole(string role)
    {
        if (!State.Roles.Contains(role))
        {
            State.Roles.Add(role);
            State.UpdatedAt = DateTime.UtcNow;
            await WriteStateAsync();
        }
    }

    public async Task RemoveRole(string role)
    {
        if (State.Roles.Remove(role))
        {
            State.UpdatedAt = DateTime.UtcNow;
            await WriteStateAsync();
        }
    }

    public Task<bool> HasRole(string role)
    {
        return Task.FromResult(State.Roles.Contains(role));
    }

    public async Task AddClaim(string type, string value)
    {
        State.Claims[type] = value;
        State.UpdatedAt = DateTime.UtcNow;
        await WriteStateAsync();
    }

    public async Task RemoveClaim(string type)
    {
        if (State.Claims.Remove(type))
        {
            State.UpdatedAt = DateTime.UtcNow;
            await WriteStateAsync();
        }
    }

    public Task<string?> GetClaim(string type)
    {
        return Task.FromResult(State.Claims.TryGetValue(type, out var value) ? value : null);
    }

    // 初始化用户（从注册调用）
    public async Task Initialize(string username, string email, string displayName, string passwordHash)
    {
        if (!string.IsNullOrEmpty(State.UserId))
            throw new AuthException("User already initialized");

        State.UserId = this.GetPrimaryKeyString();
        State.Username = username;
        State.Email = email;
        State.DisplayName = displayName;
        State.PasswordHash = passwordHash;
        State.CreatedAt = DateTime.UtcNow;
        State.UpdatedAt = DateTime.UtcNow;
        State.IsActive = true;

        // 默认角色
        State.Roles.Add("user");

        await WriteStateAsync();

        logger.LogInformation("User {UserId} initialized with username {Username}",
            State.UserId, State.Username);
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("UserAccountGrain {UserId} activated", this.GetPrimaryKeyString());
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        logger.LogDebug("UserAccountGrain {UserId} deactivated. Reason: {Reason}",
            this.GetPrimaryKeyString(), reason.Description);
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}