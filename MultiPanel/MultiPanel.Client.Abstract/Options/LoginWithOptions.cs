namespace MultiPanel.Client.Abstract.Options;

[GenerateSerializer]
[Alias("MultiPanel.Client.Abstract.Options.LoginWithOptions")]
public class LoginWithOptions
{
    [Id(0)] public bool RememberMe { get; set; } = false;

    [Id(1)]
    public string Username
    {
        get => RememberMe ? field : string.Empty;
        set;
    } = string.Empty;

    [Id(2)]
    public string PasswordHash
    {
        get => RememberMe ? field : string.Empty;
        set;
    } = string.Empty;
}