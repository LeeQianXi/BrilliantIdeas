using MultiPanel.Abstractions.DTOs;
using Orleans;

namespace MultiPanel.Interfaces.IGrains;

/// <summary>
///     用户账户域无状态 Grain，key = userName（字符串）
/// </summary>
[Alias("MultiPanel.Interfaces.IGrains.IAccountGrain")]
public interface IAccountGrain : IGrainWithStringKey
{
    /// <summary>
    ///     检测用户名是否存在
    /// </summary>
    /// <returns>是否存在用户</returns>
    [Alias("ExistAsync")]
    Task<bool> ExistAsync();

    /// <summary>
    ///     注册用户，返回双令牌
    /// </summary>
    /// <param name="passwordHash">密码Hash</param>
    /// <returns>双令牌</returns>
    [Alias("RegisterAsync")]
    Task<AuthDto> RegisterAsync(string passwordHash);

    /// <summary>
    ///     登录,校验密码，返回双令牌
    /// </summary>
    /// <param name="passwordHash">密码Hash</param>
    /// <returns>双令牌</returns>
    [Alias("LoginAsync")]
    Task<AuthDto> LoginAsync(string passwordHash);

    /// <summary>
    ///     刷新 accessToken
    /// </summary>
    /// <param name="dto">已有刷新Token</param>
    /// <returns>刷新后获取Token,失败为null</returns>
    [Alias("RefreshAsync")]
    Task<AuthDto?> RefreshAsync(AuthDto dto);

    /// <summary>
    ///     注销账户
    /// </summary>
    /// <param name="passwordHash">密码Hash</param>
    [Alias("DeleteAccountAsync")]
    Task<bool> DeleteAccountAsync(string passwordHash);
}