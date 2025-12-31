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
    /// <param name="password">用户密码Hash</param>
    /// <returns>成功返回UserId,失败返回-1</returns>
    Task<int> InsertUserAsync(string userName, string password);

    /// <summary>
    ///     验证用户名与密码
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码Hash</param>
    /// <returns>成功返回UserId,失败返回-1</returns>
    Task<int> CheckPasswordAsync(string userName, string password);

    /// <summary>
    ///     根据UserId和密码删除用户
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="password">密码Hash</param>
    /// <returns>是否成功删除</returns>
    Task<bool> DeleteUserAsync(int userId, string password);

    /// <summary>
    ///     删除用户
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码Hash</param>
    /// <returns>是否成功删除</returns>
    Task<bool> DeleteUserAsync(string userName, string password);

    /// <summary>
    ///     根据UserId获取用户实体
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns>用户实体或null</returns>
    Task<UserEntity?> GetUserByIdAsync(int userId);

    /// <summary>
    ///     根据用户名获取用户实体
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <returns>用户实体或null</returns>
    Task<UserEntity?> GetUserByNameAsync(string userName);

    /// <summary>
    ///     更新用户状态
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="isActive">用户状态</param>
    /// <returns>是否成功更新状态</returns>
    Task<bool> UpdateUserStatusAsync(int userId, bool isActive);

    /// <summary>
    ///     更新用户密码
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="newpassword">新的密码Hash</param>
    /// <returns>是否成功更新密码</returns>
    Task<bool> UpdatePasswordAsync(int userId, string newpassword);

    /// <summary>
    ///     为用户分配角色
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="roleId">角色Id</param>
    /// <returns>是否成功分配角色</returns>
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);

    /// <summary>
    ///     移除用户的角色
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="roleId">角色Id</param>
    /// <returns>是否成功移除角色</returns>
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);

    /// <summary>
    ///     获取用户的所有角色
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns>角色实体列表</returns>
    Task<List<RoleEntity>> GetUserRolesAsync(int userId);

    /// <summary>
    ///     根据角色Id获取用户列表
    /// </summary>
    /// <param name="roleId">角色Id</param>
    /// <param name="skip">跳过的用户数量</param>
    /// <param name="take">获取的用户数量</param>
    /// <returns>用户实体列表</returns>
    Task<List<UserEntity>> GetUsersByRoleAsync(int roleId, int skip = 0, int take = 100);

    /// <summary>
    ///     创建新角色
    /// </summary>
    /// <param name="roleName">角色名称</param>
    /// <param name="description">角色描述</param>
    /// <returns>成功返回RoleId,失败返回-1</returns>
    Task<int> CreateRoleAsync(string roleName, string description);

    /// <summary>
    ///     更新角色信息
    /// </summary>
    /// <param name="roleId">角色Id</param>
    /// <param name="roleName">新的角色名称</param>
    /// <param name="description">新的角色描述</param>
    /// <returns>是否成功更新角色信息</returns>
    Task<bool> UpdateRoleAsync(int roleId, string roleName, string description);

    /// <summary>
    ///     删除角色
    /// </summary>
    /// <param name="roleId">角色Id</param>
    /// <returns>是否成功删除角色</returns>
    Task<bool> DeleteRoleAsync(int roleId);

    /// <summary>
    ///     根据RoleId获取角色实体
    /// </summary>
    /// <param name="roleId">角色Id</param>
    /// <returns>角色实体或null</returns>
    Task<RoleEntity?> GetRoleByIdAsync(int roleId);

    /// <summary>
    ///     根据角色名称获取角色实体
    /// </summary>
    /// <param name="roleName">角色名称</param>
    /// <returns>角色实体或null</returns>
    Task<RoleEntity?> GetRoleByNameAsync(string roleName);

    /// <summary>
    ///     获取所有角色列表
    /// </summary>
    /// <returns>角色实体列表</returns>
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
    Task<AuthDto?> UpdateAccessTokenAsync(string refreshToken, int userId);
}