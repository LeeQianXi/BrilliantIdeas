using BCrypt.Net;

namespace MultiPanel.Shared.Services;

public interface IPasswordHasher
{
    /// <summary>
    ///     生成哈希
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="workFactor">可选的工作因子（覆盖默认值）</param>
    /// <returns>哈希后的密码字符串</returns>
    /// <exception cref="ArgumentNullException">密码为空或null</exception>
    string SaltedHash(string password, int? workFactor = null);

    /// <summary>
    ///     验证密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="hash">已哈希的密码</param>
    /// <returns>验证结果</returns>
    bool Verify(string password, string hash);

    /// <summary>
    ///     检查密码是否需要重新哈希（当工作因子提高时使用）
    /// </summary>
    /// <param name="hash">已哈希的密码</param>
    /// <param name="newWorkFactor">新的工作因子</param>
    /// <returns>是否需要重新哈希</returns>
    bool NeedsRehash(string hash, int newWorkFactor);
}

internal class BCryptPasswordHasher(int defaultWorkFactor = 12) : IPasswordHasher
{
    public string SaltedHash(string password, int? workFactor = null)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password), "密码不能为空");
        // 自动生成salt并哈希密码
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor ?? defaultWorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash)) return false;

        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
        }
        catch (SaltParseException)
        {
            // 哈希字符串格式错误
            return false;
        }
        catch (BcryptAuthenticationException)
        {
            // 密码验证失败
            return false;
        }
        catch (Exception)
        {
            // 其他异常
            return false;
        }
    }

    public bool NeedsRehash(string hash, int newWorkFactor)
    {
        if (string.IsNullOrWhiteSpace(hash)) return false;

        try
        {
            // 从哈希值中提取当前的工作因子
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, newWorkFactor);
        }
        catch
        {
            // 如果解析失败，可能需要重新哈希
            return true;
        }
    }
}