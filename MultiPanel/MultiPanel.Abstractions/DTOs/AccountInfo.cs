using Orleans;

namespace MultiPanel.Abstractions.DTOs;

[GenerateSerializer]
[Alias("MultiPanel.Abstractions.DTOs.AccountInfo")]
public class AccountInfo
{
    [Id(0)] public string UserName { get; set; } = string.Empty;
    [Id(1)] public int UserId { get; set; }
    public static AccountInfo Empty => new();
}