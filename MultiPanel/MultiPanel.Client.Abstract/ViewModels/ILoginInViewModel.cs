using System.Reactive;
using CommunityToolkit.Mvvm.Input;
using DIAbstract.Services;
using MultiPanel.Abstractions.DTOs;
using ReactiveUI;

namespace MultiPanel.Client.Abstract.ViewModels;

public interface ILoginInViewModel : IDependencyInjection
{
    /// <summary>
    ///     用于绑定,传递<see cref="RememberMe" />
    /// </summary>
    bool RememberMe { get; set; }

    /// <summary>
    ///     用于绑定,用于传递<see cref="Username" />
    /// </summary>
    string Username { get; set; }

    /// <summary>
    ///     用于绑定,用于获取<see cref="Password" />
    /// </summary>
    string Password { get; set; }

    /// <summary>
    ///     用于绑定,用于处理登陆事件
    /// </summary>
    public IAsyncRelayCommand LoginCommand { get; }

    /// <summary>
    ///     用于传递警告信息
    /// </summary>
    public Interaction<string, Unit> WarningInfo { get; }

    public Task<AccountInfo> TryLoginWithLastedData();
}