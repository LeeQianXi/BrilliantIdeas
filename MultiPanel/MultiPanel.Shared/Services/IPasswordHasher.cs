namespace MultiPanel.Shared.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
    bool NeedsRehash(string hash);
}

internal class BCryptPasswordHasher(int workFactor = 12) : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
    }

    public bool NeedsRehash(string hash)
    {
        return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, workFactor);
    }
}