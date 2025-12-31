using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiPanel.Abstractions.DTOs;
using MultiPanel.Abstractions.Entities;
using MultiPanel.Abstractions.IRepository;
using MultiPanel.Shared.Services;
using MySql.Data.MySqlClient;
using StackExchange.Redis;

namespace MultiPanel.Grains.Persistence;

internal sealed partial class AccountRepository(
    IConfiguration configuration,
    ILogger<AccountRepository> logger,
    IConnectionMultiplexer redisConn,
    ITokenGenerator tokenGenerator,
    IPasswordHasher passwordHasher)
    : IAccountRepository
{
    private readonly string _mysqlConnectionString = configuration.GetConnectionString("Mysql")!;
    private ILogger<AccountRepository> Logger { get; } = logger;
    private ITokenGenerator TokenGenerator { get; } = tokenGenerator;
    private IPasswordHasher PasswordHasher { get; } = passwordHasher;

    //TODO MySQL
    [field: AllowNull]
    private MySqlConnection MySqlConnection
    {
        get
        {
            if (field?.State is ConnectionState.Open) return field;
            field?.Dispose();
            field = new MySqlConnection(_mysqlConnectionString);
            field.Open();
            return field;
        }
    }

    private IDatabase Redis { get; } = redisConn.GetDatabase();

    public async Task<bool> ExistUserAsync(string userName)
    {
        const string sql = """
                           SELECT COUNT(1) 
                           FROM Users 
                           WHERE IsEnable = TRUE AND UserName = @UserName
                           """;
        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserName", userName);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            LogErrorCheckingIfUserExistsUsername(Logger, ex, userName);
            throw;
        }
    }

    public async Task<int> InsertUserAsync(string userName, string passwordHash)
    {
        var saltHash = PasswordHasher.SaltedHash(passwordHash);
        const string sql = """
                           INSERT INTO Users (UserName, SaltPasswordHash)
                           VALUES (@UserName, @SaltPasswordHash);
                           SELECT LAST_INSERT_ID();
                           """;
        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserName", userName);
            command.Parameters.AddWithValue("@SaltPasswordHash", saltHash);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            LogUserAlreadyExistsUsername(Logger, userName);
            return -1;
        }
        catch (Exception ex)
        {
            LogErrorInsertingUserUsername(Logger, ex, userName);
            return -1;
        }
    }

    public async Task<int> CheckPasswordAsync(string userName, string passwordHash)
    {
        const string sql = """
                           SELECT UserId, SaltPasswordHash
                           FROM Users 
                           WHERE IsEnable = TRUE AND IsEnable = TRUE AND UserName = @UserName AND IsActive = TRUE
                           """;
        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserName", userName);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return -1;
            var storedSaltHash = reader.GetString("SaltPasswordHash");
            if (!PasswordHasher.Verify(passwordHash, storedSaltHash)) return -1;
            return reader.GetInt32("UserId");
        }
        catch (Exception ex)
        {
            LogErrorCheckingPasswordForUserUsername(Logger, ex, userName);
            return -1;
        }
    }

    public async Task<bool> DeleteUserAsync(int userId, string passwordHash)
    {
        var saltHash = PasswordHasher.SaltedHash(passwordHash);
        const string sql = """
                           UPDATE Users
                           SET IsEnable=0
                           WHERE UserId = @UserId AND SaltPasswordHash = @SaltPasswordHash;
                           """;
        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@SaltPasswordHash", saltHash);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            LogErrorDeletingUserUserid(Logger, ex, userId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(string userName, string passwordHash)
    {
        var saltHash = PasswordHasher.SaltedHash(passwordHash);
        const string sql = """
                           UPDATE Users
                           SET IsEnable=0
                           WHERE UserName = @UserName AND SaltPasswordHash = @SaltPasswordHash;
                           """;
        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserName", userName);
            command.Parameters.AddWithValue("@SaltPasswordHash", saltHash);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            LogErrorDeletingUserUsername(Logger, ex, userName);
            return false;
        }
    }

    public async Task<UserEntity?> GetUserByIdAsync(int userId)
    {
        const string userRoleSql = """
                                   SELECT  u.UserId,
                                           u.UserName,
                                           u.SaltPasswordHash,
                                           u.IsActive,
                                           u.CreatedAt,
                                           u.UpdatedAt,
                                           JSON_ARRAYAGG(r.RoleName) AS RoleNames
                                   FROM Users u
                                   LEFT JOIN UserRoles ur ON ur.UserId = u.UserId
                                   LEFT JOIN Roles     r  ON r.RoleId  = ur.RoleId
                                   WHERE u.IsEnable=TRUE AND u.UserId = @UserId
                                   GROUP BY u.UserId;
                                   """;
        try
        {
            await EnsureConnectionOpenAsync();

            // 获取用户信息
            await using var userCommand = new MySqlCommand(userRoleSql, MySqlConnection);
            userCommand.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await userCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            var user = new UserEntity
            {
                UserId = reader.GetInt32("UserId"),
                UserName = reader.GetString("UserName"),
                SaltPasswordHash = reader.GetString("SaltPasswordHash"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt")
            };

            var rolesJson = reader["RoleNames"] as string;
            if (!string.IsNullOrEmpty(rolesJson) && rolesJson != "null")
                user.UserRoles.AddRange(JsonSerializer.Deserialize<List<string>>(rolesJson)!);

            return user;
        }
        catch (Exception ex)
        {
            LogErrorGettingUserByIdUserid(Logger, ex, userId);
            return null;
        }
    }

    public async Task<UserEntity?> GetUserByNameAsync(string userName)
    {
        const string userRoleSql = """
                                   SELECT  u.UserId,
                                           u.UserName,
                                           u.SaltPasswordHash,
                                           u.IsActive,
                                           u.CreatedAt,
                                           u.UpdatedAt,
                                           JSON_ARRAYAGG(r.RoleName) AS RoleNames
                                   FROM Users u
                                   LEFT JOIN UserRoles ur ON ur.UserId = u.UserId
                                   LEFT JOIN Roles     r  ON r.RoleId  = ur.RoleId
                                   WHERE u.IsEnable=TRUE AND u.UserName = @UserName
                                   GROUP BY u.UserId;
                                   """;
        try
        {
            await EnsureConnectionOpenAsync();

            // 获取用户信息
            await using var userCommand = new MySqlCommand(userRoleSql, MySqlConnection);
            userCommand.Parameters.AddWithValue("@UserName", userName);

            await using var reader = await userCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            var user = new UserEntity
            {
                UserId = reader.GetInt32("UserId"),
                UserName = reader.GetString("UserName"),
                SaltPasswordHash = reader.GetString("SaltPasswordHash"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt")
            };
            var rolesJson = reader["RoleNames"] as string;
            if (!string.IsNullOrEmpty(rolesJson) && rolesJson != "null")
                user.UserRoles.AddRange(JsonSerializer.Deserialize<List<string>>(rolesJson)!);
            return user;
        }
        catch (Exception ex)
        {
            LogErrorGettingUserByNameUsername(Logger, ex, userName);
            return null;
        }
    }

    public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
    {
        const string sql = """
                           UPDATE Users 
                           SET IsActive = @IsActive, UpdatedAt = @UpdatedAt
                           WHERE IsEnable=TRUE AND UserId = @UserId
                           """;
        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@IsActive", isActive);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            LogErrorUpdatingUserStatusUserid(Logger, ex, userId);
            return false;
        }
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        var newFinalHash = PasswordHasher.SaltedHash(newPasswordHash);
        const string sql = """
                           UPDATE Users 
                           SET SaltPasswordHash = @SaltPasswordHash, 
                               UpdatedAt = @UpdatedAt
                           WHERE IsEnable=TRUE AND UserId = @UserId
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@SaltPasswordHash", newFinalHash);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            LogErrorUpdatingPasswordForUserUserid(Logger, ex, userId);
            return false;
        }
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        const string sql = """
                           INSERT INTO UserRoles (UserId, RoleId)
                           SELECT @UserId, @RoleId
                           FROM dual
                           WHERE EXISTS ( SELECT 1 FROM Users WHERE UserId = @UserId AND IsEnable = TRUE);
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            LogErrorAssigningRoleRoleIdToUserUserid(Logger, ex, roleId, userId);
            return false;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        const string sql = "DELETE FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId";

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            LogErrorRemovingRoleRoleIdFromUserUserid(Logger, ex, roleId, userId);
            return false;
        }
    }

    public async Task<List<RoleEntity>> GetUserRolesAsync(int userId)
    {
        const string sql = """
                           SELECT r.RoleId, r.RoleName, r.Description, r.CreatedAt
                           FROM UserRoles ur
                           JOIN Roles r ON ur.RoleId = r.RoleId
                           WHERE ur.UserId = @UserId
                           ORDER BY r.RoleName
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@UserId", userId);

            var roles = new List<RoleEntity>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                roles.Add(new RoleEntity
                {
                    RoleId = reader.GetInt32("RoleId"),
                    RoleName = reader.GetString("RoleName"),
                    Description = reader.GetString("Description"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });

            return roles;
        }
        catch (Exception ex)
        {
            LogErrorGettingRolesForUserUserid(Logger, ex, userId);
            return [];
        }
    }

    public async Task<List<UserEntity>> GetUsersByRoleAsync(int roleId, int skip = 0, int take = 100)
    {
        const string sql = """
                           SELECT u.UserId, u.UserName, u.SaltPasswordHash, u.IsActive, u.CreatedAt, u.UpdatedAt
                           FROM UserRoles ur
                           JOIN Users u ON ur.UserId = u.UserId
                           WHERE ur.RoleId = @RoleId AND u.IsEnable = TRUE = u.IsActive = TRUE
                           ORDER BY u.UserId DESC
                           LIMIT @Skip, @Take
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@RoleId", roleId);
            command.Parameters.AddWithValue("@Skip", skip);
            command.Parameters.AddWithValue("@Take", take);

            var users = new List<UserEntity>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var user = new UserEntity
                {
                    UserId = reader.GetInt32("UserId"),
                    UserName = reader.GetString("UserName"),
                    SaltPasswordHash = reader.GetString("SaltPasswordHash"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                };
                users.Add(user);
            }

            return users;
        }
        catch (Exception ex)
        {
            LogErrorGettingUsersByRoleRoleId(Logger, ex, roleId);
            return [];
        }
    }

    public async Task<int> CreateRoleAsync(string roleName, string description)
    {
        const string sql = """
                           INSERT INTO Roles (RoleName, Description)
                           VALUES (@RoleName, @Description);
                           SELECT LAST_INSERT_ID();
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@RoleName", roleName);
            command.Parameters.AddWithValue("@Description", description);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            LogRoleAlreadyExistsRoleName(Logger, roleName);
            return -1;
        }
        catch (Exception ex)
        {
            LogErrorCreatingRoleRoleName(Logger, ex, roleName);
            return -1;
        }
    }

    public async Task<bool> UpdateRoleAsync(int roleId, string roleName, string description)
    {
        const string sql = """
                           UPDATE Roles 
                           SET RoleName = @RoleName, Description = @Description
                           WHERE RoleId = @RoleId
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@RoleId", roleId);
            command.Parameters.AddWithValue("@RoleName", roleName);
            command.Parameters.AddWithValue("@Description", description);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            LogRoleNameAlreadyExistsRoleName(Logger, roleName);
            return false;
        }
        catch (Exception ex)
        {
            LogErrorUpdatingRoleRoleId(Logger, ex, roleId);
            return false;
        }
    }

    public async Task<bool> DeleteRoleAsync(int roleId)
    {
        const string sql = "DELETE FROM Roles WHERE RoleId = @RoleId";

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@RoleId", roleId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            LogErrorDeletingRoleRoleId(Logger, ex, roleId);
            return false;
        }
    }

    public async Task<RoleEntity?> GetRoleByIdAsync(int roleId)
    {
        const string sql = """
                           SELECT RoleId, RoleName, Description, CreatedAt
                           FROM Roles 
                           WHERE RoleId = @RoleId
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@RoleId", roleId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return new RoleEntity
                {
                    RoleId = reader.GetInt32("RoleId"),
                    RoleName = reader.GetString("RoleName"),
                    Description = reader.GetString("Description"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };

            return null;
        }
        catch (Exception ex)
        {
            LogErrorGettingRoleByIdRoleId(Logger, ex, roleId);
            return null;
        }
    }

    public async Task<RoleEntity?> GetRoleByNameAsync(string roleName)
    {
        const string sql = """
                           SELECT RoleId, RoleName, Description, CreatedAt
                           FROM Roles 
                           WHERE RoleName = @RoleName
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            command.Parameters.AddWithValue("@RoleName", roleName);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return new RoleEntity
                {
                    RoleId = reader.GetInt32("RoleId"),
                    RoleName = reader.GetString("RoleName"),
                    Description = reader.GetString("Description"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };

            return null;
        }
        catch (Exception ex)
        {
            LogErrorGettingRoleByNameRoleName(Logger, ex, roleName);
            return null;
        }
    }

    public async Task<List<RoleEntity>> GetAllRolesAsync()
    {
        const string sql = """
                           SELECT RoleId, RoleName, Description, CreatedAt
                           FROM Roles 
                           ORDER BY RoleName
                           """;

        try
        {
            await EnsureConnectionOpenAsync();
            await using var command = new MySqlCommand(sql, MySqlConnection);
            var roles = new List<RoleEntity>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                roles.Add(new RoleEntity
                {
                    RoleId = reader.GetInt32("RoleId"),
                    RoleName = reader.GetString("RoleName"),
                    Description = reader.GetString("Description"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });

            return roles;
        }
        catch (Exception ex)
        {
            LogErrorGettingAllRoles(Logger, ex);
            return [];
        }
    }

    //TODO Redis
    public async Task<AuthDto> GenerateTokenAsync(int userId, TimeSpan ttl)
    {
        var userKey = GenerateUserKey(userId);
        var (rt, rdf) = TokenGenerator.GeneratorRefreshToken(userId);
        var (at, adf) = TokenGenerator.GeneratorAccessToken(userId, (await GetUserByIdAsync(userId))?.UserRoles ?? []);
        await PutRedisHash(userKey, Keys.RefreshToken, rt, rdf.UtcDateTime);
        await PutRedisHash(userKey, Keys.AccessToken, at, adf.UtcDateTime);
        return new AuthDto
        {
            UserId = userId,
            AccessToken = at,
            RefreshToken = rt,
            ExpiresAt = rdf
        };
    }

    public async Task<bool> RemoveRefreshTokenAsync(string refreshToken, int userId)
    {
        var userKey = GenerateUserKey(userId);
        var value = await Redis.HashGetAsync(userKey, Keys.RefreshToken);
        if (!value.HasValue || value.ToString() != refreshToken) return false;
        await Redis.HashDeleteAsync(userKey, Keys.RefreshToken);
        return true;
    }

    public async Task<bool> RemoveAccessTokenAsync(string accessToken, int userId)
    {
        var userKey = GenerateUserKey(userId);
        var value = await Redis.HashGetAsync(userKey, Keys.AccessToken);
        if (!value.HasValue || value.ToString() != accessToken) return false;
        await Redis.HashDeleteAsync(userKey, Keys.AccessToken);
        return true;
    }

    public async Task RemoveAllTokensAsync(int userId)
    {
        await Redis.KeyDeleteAsync(GenerateUserKey(userId));
    }

    public async Task<bool> VerifyAccessTokenAsync(string accessToken, int userId)
    {
        var value = await Redis.HashGetAsync(GenerateUserKey(userId), Keys.AccessToken);
        return value.HasValue && value.ToString() == accessToken;
    }

    public async Task<AuthDto?> UpdateAccessTokenAsync(string refreshToken, int userId)
    {
        var userKey = GenerateUserKey(userId);
        var value = await Redis.HashGetAsync(userKey, Keys.RefreshToken);
        if (!value.HasValue || value.ToString() != refreshToken) return null;
        var (rt, rdf) = TokenGenerator.GeneratorRefreshToken(userId);
        var (at, adf) = TokenGenerator.GeneratorAccessToken(userId, (await GetUserByIdAsync(userId))?.UserRoles ?? []);
        await PutRedisHash(userKey, Keys.RefreshToken, rt, rdf.UtcDateTime);
        await PutRedisHash(userKey, Keys.AccessToken, at, adf.UtcDateTime);
        return new AuthDto
        {
            UserId = userId,
            AccessToken = at,
            RefreshToken = rt,
            ExpiresAt = rdf
        };
    }

    //TODO Private Methods
    private async Task PutRedisHash(RedisKey key, RedisValue field, RedisValue value, DateTime? ttl = null)
    {
        await Redis.HashSetAsync(key, field, value);
        if (ttl is not null)
            await Redis.HashFieldExpireAsync(key, [field], ttl.Value);
    }

    private async Task PutRedisHash(RedisKey key, HashEntry[] entries, DateTime? ttl = null)
    {
        await Redis.HashSetAsync(key, entries);
        if (ttl is not null)
            await Redis.HashFieldExpireAsync(key, entries.GetKeys(), ttl.Value);
    }

    private static RedisKey GenerateUserKey(int userId)
    {
        return new RedisKey($"userinfo/data/u{userId}");
    }

    private async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken = default)
    {
        if (MySqlConnection.State != ConnectionState.Open) await MySqlConnection.OpenAsync(cancellationToken);
    }

    private static class Keys
    {
        public const string RefreshToken = "REFRESH_TOKEN";
        public const string AccessToken = "ACCESS_TOKEN";
    }
}