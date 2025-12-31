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
    /// <param name="password">密码Hash</param>
    /// <returns>双令牌</returns>
    /// <exception cref="InvalidOperationException">用户名已存在</exception>
    [Alias("RegisterAsync")]
    Task<AuthDto> RegisterAsync(string password);

    /// <summary>
    ///     登录,校验密码，返回双令牌
    /// </summary>
    /// <param name="password">密码Hash</param>
    /// <returns>双令牌</returns>
    /// <exception cref="InvalidOperationException">用户名不存在或密码错误</exception>
    [Alias("LoginAsync")]
    Task<AuthDto> LoginAsync(string password);

    /// <summary>
    ///     刷新 accessToken
    /// </summary>
    /// <param name="dto">已有刷新Token</param>
    /// <returns>刷新后获取Token,失败为null</returns>
    /// <exception cref="InvalidOperationException">不合法的Token</exception>
    [Alias("RefreshAsync")]
    Task<AuthDto> RefreshAsync(AuthDto dto);

    /// <summary>
    ///     注销账户
    ///     <br />用户名不存在时，返回false
    /// </summary>
    /// <param name="password">密码Hash</param>
    [Alias("DeleteAccountAsync")]
    Task<bool> DeleteAccountAsync(string password);
}