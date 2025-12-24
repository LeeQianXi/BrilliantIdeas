using MultiPanel.Abstractions.DTOs;
using Orleans;

namespace MultiPanel.Interfaces.IGrains;

/// 用户账户Grain
[Alias("MultiPanel.Interfaces.IGrains.IUserAccountGrain")]
public interface IUserAccountGrain : IGrainWithStringKey
{
    // 查询操作
    [Alias("GetUserInfo")]
    Task<UserInfo> GetUserInfo();

    [Alias("Exists")]
    Task<bool> Exists();

    [Alias("GetActiveSessions")]
    Task<List<Guid>> GetActiveSessions();

    // 认证操作
    [Alias("ValidatePassword")]
    Task<bool> ValidatePassword(string password);

    [Alias("IsLockedOut")]
    Task<bool> IsLockedOut();

    [Alias("ResetFailedLoginAttempts")]
    Task ResetFailedLoginAttempts();

    [Alias("IncrementFailedLoginAttempts")]
    Task IncrementFailedLoginAttempts();

    // 更新操作
    [Alias("UpdateLastLogin")]
    Task UpdateLastLogin();

    [Alias("UpdateProfile")]
    Task UpdateProfile(string? displayName, string? email);

    [Alias("ChangePassword")]
    Task ChangePassword(string oldPassword, string newPassword);

    [Alias("SetPasswordHash")]
    Task SetPasswordHash(string passwordHash);

    // 会话管理
    [Alias("AddSession")]
    Task AddSession(Guid sessionId);

    [Alias("RemoveSession")]
    Task RemoveSession(Guid sessionId);

    // 状态管理
    [Alias("Activate")]
    Task Activate();

    [Alias("Deactivate")]
    Task Deactivate(string reason);

    [Alias("ToggleEmailVerified")]
    Task<bool> ToggleEmailVerified(bool verified);

    // 角色和声明管理
    [Alias("AddRole")]
    Task AddRole(string role);

    [Alias("RemoveRole")]
    Task RemoveRole(string role);

    [Alias("HasRole")]
    Task<bool> HasRole(string role);

    [Alias("AddClaim")]
    Task AddClaim(string type, string value);

    [Alias("RemoveClaim")]
    Task RemoveClaim(string type);

    [Alias("GetClaim")]
    Task<string?> GetClaim(string type);

    [Alias("Initialize")]
    Task Initialize(string username, string email, string displayName, string passwordHash);
}