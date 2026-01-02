using Microsoft.Extensions.Logging;

namespace MultiPanel.Grains.Persistence;

internal partial class AccountRepository
{
    [LoggerMessage(LogLevel.Error, "Error checking if user exists: {UserName}")]
    static partial void LogErrorCheckingIfUserExistsUsername(ILogger<AccountRepository> logger, Exception ex,
        string userName);

    [LoggerMessage(LogLevel.Warning, "User already exists: {UserName}")]
    static partial void LogUserAlreadyExistsUsername(ILogger<AccountRepository> logger, string userName);

    [LoggerMessage(LogLevel.Error, "Error inserting user: {UserName}")]
    static partial void LogErrorInsertingUserUsername(ILogger<AccountRepository> logger, Exception ex, string userName);

    [LoggerMessage(LogLevel.Error, "Error checking password for user: {UserName}")]
    static partial void LogErrorCheckingPasswordForUserUsername(ILogger<AccountRepository> logger, Exception ex,
        string userName);

    [LoggerMessage(LogLevel.Error, "Error deleting user: {UserId}")]
    static partial void LogErrorDeletingUserUserid(ILogger<AccountRepository> logger, Exception ex, int userId);

    [LoggerMessage(LogLevel.Error, "Error deleting user: {UserName}")]
    static partial void LogErrorDeletingUserUsername(ILogger<AccountRepository> logger, Exception ex, string userName);

    [LoggerMessage(LogLevel.Error, "Error getting user by ID: {UserId}")]
    static partial void LogErrorGettingUserByIdUserid(ILogger<AccountRepository> logger, Exception ex, int userId);

    [LoggerMessage(LogLevel.Error, "Error getting user by name: {UserName}")]
    static partial void LogErrorGettingUserByNameUsername(ILogger<AccountRepository> logger, Exception ex,
        string userName);

    [LoggerMessage(LogLevel.Error, "Error updating user status: {UserId}")]
    static partial void LogErrorUpdatingUserStatusUserid(ILogger<AccountRepository> logger, Exception ex, int userId);

    [LoggerMessage(LogLevel.Error, "Error updating password for user: {UserId}")]
    static partial void LogErrorUpdatingPasswordForUserUserid(ILogger<AccountRepository> logger, Exception ex,
        int userId);

    [LoggerMessage(LogLevel.Error, "Error assigning role {RoleId} to user {UserId}")]
    static partial void LogErrorAssigningRoleRoleIdToUserUserid(ILogger<AccountRepository> logger, Exception ex,
        int roleId, int userId);

    [LoggerMessage(LogLevel.Error, "Error removing role {RoleId} from user {UserId}")]
    static partial void LogErrorRemovingRoleRoleIdFromUserUserid(ILogger<AccountRepository> logger, Exception ex,
        int roleId, int userId);

    [LoggerMessage(LogLevel.Error, "Error getting roles for user: {UserId}")]
    static partial void LogErrorGettingRolesForUserUserid(ILogger<AccountRepository> logger, Exception ex, int userId);

    [LoggerMessage(LogLevel.Error, "Error getting users by role: {RoleId}")]
    static partial void LogErrorGettingUsersByRoleRoleId(ILogger<AccountRepository> logger, Exception ex, int roleId);

    [LoggerMessage(LogLevel.Warning, "Role already exists: {RoleName}")]
    static partial void LogRoleAlreadyExistsRoleName(ILogger<AccountRepository> logger, string roleName);

    [LoggerMessage(LogLevel.Error, "Error creating role: {RoleName}")]
    static partial void LogErrorCreatingRoleRoleName(ILogger<AccountRepository> logger, Exception ex, string roleName);

    [LoggerMessage(LogLevel.Warning, "Role name already exists: {RoleName}")]
    static partial void LogRoleNameAlreadyExistsRoleName(ILogger<AccountRepository> logger, string roleName);

    [LoggerMessage(LogLevel.Error, "Error updating role: {RoleId}")]
    static partial void LogErrorUpdatingRoleRoleId(ILogger<AccountRepository> logger, Exception ex, int roleId);

    [LoggerMessage(LogLevel.Error, "Error deleting role: {RoleId}")]
    static partial void LogErrorDeletingRoleRoleId(ILogger<AccountRepository> logger, Exception ex, int roleId);

    [LoggerMessage(LogLevel.Error, "Error getting role by ID: {RoleId}")]
    static partial void LogErrorGettingRoleByIdRoleId(ILogger<AccountRepository> logger, Exception ex, int roleId);

    [LoggerMessage(LogLevel.Error, "Error getting role by name: {RoleName}")]
    static partial void LogErrorGettingRoleByNameRoleName(ILogger<AccountRepository> logger, Exception ex,
        string roleName);

    [LoggerMessage(LogLevel.Error, "Error getting all roles")]
    static partial void LogErrorGettingAllRoles(ILogger<AccountRepository> logger, Exception ex);
}