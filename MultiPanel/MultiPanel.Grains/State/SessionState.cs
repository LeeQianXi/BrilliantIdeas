namespace MultiPanel.Grains.State;

/// <summary>
///     会话的内部状态信息
/// </summary>
[GenerateSerializer]
[Alias("MultiPanel.Grains.State.SessionState")]
public class SessionState
{
    [Id(0)] public int UserId { get; set; }
    [Id(1)] public string AccessToken { get; set; } = string.Empty;

    public void Reset(int userId)
    {
        UserId = userId;
        AccessToken = string.Empty;
    }

    public bool IsValid()
    {
        return UserId > 0
               && !string.IsNullOrWhiteSpace(AccessToken);
    }
}