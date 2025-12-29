using MultiPanel.Abstractions.DTOs;
using MultiPanel.Abstractions.Entities;

namespace MultiPanel.Abstractions.IRepository;

public interface IAccountRepository
{
    //TODO MySQL
    /// <summary>
    ///     检测是否存在用户
    /// </summary>
    /// <param name="userName">被检测用户名</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistUserAsync(string userName);

    /// <summary>
    ///     插入新用户
    ///     <br />用户名已存在将会失败
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="passwordHash">用户密码Hash</param>
    /// <returns>成功返回UserId,失败返回-1</returns>
    Task<int> InsertUserAsync(string userName, string passwordHash);

    /// <summary>
    ///     验证用户名与密码
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="passwordHash">密码Hash</param>
    /// <returns>成功返回UserId,失败返回-1</returns>
    Task<int> CheckPasswordAsync(string userName, string passwordHash);

    Task<bool> DeleteUserAsync(int userId, string passwordHash);

    /// <summary>
    ///     删除用户
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="passwordHash">密码Hash</param>
    /// <returns>是否成功删除</returns>
    Task<bool> DeleteUserAsync(string userName, string passwordHash);

    Task<UserEntity?> GetUserByIdAsync(int userId);
    Task<UserEntity?> GetUserByNameAsync(string userName);
    Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
    Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<List<RoleEntity>> GetUserRolesAsync(int userId);
    Task<List<UserEntity>> GetUsersByRoleAsync(int roleId, int skip = 0, int take = 100);
    Task<int> CreateRoleAsync(string roleName, string description);
    Task<bool> UpdateRoleAsync(int roleId, string roleName, string description);
    Task<bool> DeleteRoleAsync(int roleId);
    Task<RoleEntity?> GetRoleByIdAsync(int roleId);
    Task<RoleEntity?> GetRoleByNameAsync(string roleName);

    Task<List<RoleEntity>> GetAllRolesAsync();

    //TODO Redis
    //TODO 无感刷新
    /// <summary>
    ///     生成并保存刷新Token
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="ttl">过期时间</param>
    /// <param name="roles">用户role</param>
    Task<AuthDto> GenerateTokenAsync(int userId, TimeSpan ttl);

    /// <summary>
    ///     生成并保存刷新Token
    ///     <br />使用默认TTL时间
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="roles">用户role</param>
    Task<AuthDto> GenerateTokenAsync(int userId)
    {
        return GenerateTokenAsync(userId, TimeSpan.FromDays(1));
    }

    /// <summary>
    ///     移除刷新Token
    /// </summary>
    /// <param name="refreshToken">被移除Token</param>
    /// <param name="userId">用户Id</param>
    Task<bool> RemoveRefreshTokenAsync(string refreshToken, int userId);

    /// <summary>
    ///     强制过期访问Token
    /// </summary>
    /// <param name="accessToken">被移除Token</param>
    /// <param name="userId">用户Id</param>
    Task<bool> RemoveAccessTokenAsync(string accessToken, int userId);

    /// <summary>
    ///     移除指定<see cref="userId" />所有Token
    /// </summary>
    /// <param name="userId">用户Id</param>
    Task RemoveAllTokensAsync(int userId);

    /// <summary>
    ///     验证Token有效性
    /// </summary>
    /// <param name="accessToken">被验证Token</param>
    /// <param name="userId">用户Id</param>
    /// <returns>是否验证成功</returns>
    Task<bool> VerifyAccessTokenAsync(string accessToken, int userId);

    /// <summary>
    ///     通过RefreshToken更新Token
    /// </summary>
    /// <param name="refreshToken">刷新用Token</param>
    /// <param name="userId">用户Id</param>
    /// <returns>新的访问Token</returns>
    Task<string> UpdateAccessTokenAsync(string refreshToken, int userId);
}