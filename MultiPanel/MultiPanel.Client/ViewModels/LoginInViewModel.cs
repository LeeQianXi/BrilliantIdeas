using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiPanel.Abstractions.DTOs;
using MultiPanel.Client.Abstract.Bases;
using MultiPanel.Client.Abstract.Options;
using MultiPanel.Client.Abstract.Services;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Client.Abstract.Views;
using MultiPanel.Client.Orleans;
using MultiPanel.Interfaces.IGrains;
using MultiPanel.Shared.Utils;
using ReactiveUI;

namespace MultiPanel.Client.ViewModels;

public partial class LoginInViewModel : ViewModelBase, ILoginInViewModel
{
    private readonly IWritableConfigure<LoginWithOptions> _configure;

    /// <inheritdoc />
    public LoginInViewModel(IServiceProvider serviceProvider)
    {
        _configure = serviceProvider
            .GetRequiredService<IWritableConfigureFactory>()
            .GetConfigure<LoginWithOptions>(Path.Combine(ServiceLocator.ApplicationDataFolder, "config.json"));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<LoginInViewModel>>();
        RememberMe = _configure.Value.RememberMe;
        Username = _configure.Value.Username;
    }

    private string PasswordHash
    {
        get => _configure.Value.PasswordHash;
        set => _configure.Value.PasswordHash = value;
    }

    public override IServiceProvider ServiceProvider { get; }
    public override ILogger Logger { get; }

    public bool RememberMe
    {
        get;
        set => field = _configure.Value.RememberMe = value;
    }

    public string Username
    {
        get;
        set => field = _configure.Value.Username = value;
    }

    public string Password
    {
        get => field ?? string.Empty;
        set;
    }

    public Interaction<string, Unit> DisplayToScreen { get; } = new();

    public Interaction<IMainMenuView, Unit> SuccessLoginInteraction { get; } = new();

    public async Task<AuthDto?> LoginWithRememberMeAsync()
    {
        if (!RememberMe) return null;
        var client = ServiceProvider.GetRequiredService<IClusterClient>();
        var ag = client.GetGrain<IAccountGrain>(Username);
        try
        {
            return await ag.LoginAsync(PasswordHash);
        }
        catch (Exception ex)
        {
            LogFailedToLoginToServer(Logger, ex);
            return null;
        }
    }


    [RelayCommand]
    private async Task LoginClick()
    {
        var validator = ServiceProvider.GetRequiredService<IValidator<LoginInViewModel>>();
        var result = await validator.ValidateAsync(this);
        if (!result.IsValid)
        {
            var validateInfo = result.Errors.First();
            await DisplayToScreen.Handle(validateInfo.ErrorMessage);
            LogValidateFormDataErrorErrormessage(Logger, validateInfo.ErrorMessage);
            return;
        }

        Logger.LogInformation("Trying to login to server");
        var clientContext = ServiceProvider.GetRequiredService<IClientContext>();
        if (!clientContext.IsConnected)
        {
            Logger.LogError("Failed Connect to server");
            await DisplayToScreen.Handle("无法服务器,请稍候再试");
            return;
        }

        var ag = clientContext.Client.GetGrain<IAccountGrain>(Username);
        if (!await ag.ExistAsync())
        {
            await DisplayToScreen.Handle("用户名不存在");
            Logger.LogWarning("Try to login with a non-exist account");
            return;
        }

        var pwdHash = Password.Hash(SHA384.Create());
        PasswordHash = pwdHash;
        AuthDto auth;
        try
        {
            auth = await ag.LoginAsync(pwdHash);
        }
        catch (Exception ex)
        {
            await DisplayToScreen.Handle("用户名或密码错误");
            LogFailedToLoginToServer(Logger, ex);
            return;
        }

        await SuccessLoginCommand.ExecuteAsync(auth);
    }

    [RelayCommand]
    private async Task RegisterClick()
    {
        var validator = ServiceProvider.GetRequiredService<IValidator<LoginInViewModel>>();
        var result = await validator.ValidateAsync(this);
        if (!result.IsValid)
        {
            var validateInfo = result.Errors.First();
            await DisplayToScreen.Handle(validateInfo.ErrorMessage);
            LogValidateFormDataErrorErrormessage(Logger, validateInfo.ErrorMessage);
            return;
        }

        Logger.LogInformation("Trying to register a new account");
        var clientContext = ServiceProvider.GetRequiredService<IClientContext>();
        if (!clientContext.IsConnected)
        {
            Logger.LogError("Failed Connect to server");
            await DisplayToScreen.Handle("无法服务器,请稍候再试");
            return;
        }

        var ag = clientContext.Client.GetGrain<IAccountGrain>(Username);
        if (!await ag.ExistAsync())
        {
            await DisplayToScreen.Handle("用户名已被使用");
            Logger.LogWarning("Try to register with a existed account");
            return;
        }

        var pwdHash = Password.Hash(SHA384.Create());
        PasswordHash = pwdHash;
        AuthDto auth;
        try
        {
            auth = await ag.RegisterAsync(pwdHash);
        }
        catch (Exception ex)
        {
            await DisplayToScreen.Handle("注册失败,请稍候再试");
            Logger.LogError(ex, "Failed to register new account");
            return;
        }

        await SuccessLoginCommand.ExecuteAsync(auth);
    }

    [RelayCommand]
    private async Task SuccessLogin(AuthDto auth)
    {
        Logger.LogInformation("Successful Verify Account");
        var vm = ServiceLocator.Instance.MainMenuViewModel;
        vm.Auth = auth;
        vm.UserName = Username;
        Logger.LogInformation("Try to Open MainMenu Window");
        await SuccessLoginInteraction.Handle(ServiceLocator.Instance.MainMenuView);
    }

    [RelayCommand]
    private void SaveConfig()
    {
        _configure.SaveToFile();
    }

    [LoggerMessage(LogLevel.Warning, "Validate Form data Error:{ErrorMessage}")]
    static partial void LogValidateFormDataErrorErrormessage(ILogger logger, string errorMessage);

    [LoggerMessage(LogLevel.Error, "Failed to login to server")]
    static partial void LogFailedToLoginToServer(ILogger logger, Exception ex);
}

/// <summary>
///     <see cref="LoginInViewModel" />的登陆信息验证器
/// </summary>
internal class LoginInValidator : AbstractValidator<LoginInViewModel>
{
    public LoginInValidator()
    {
        RuleFor(vm => vm.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("用户名只能包含字母、数字、下划线");
        RuleFor(vm => vm.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(8).WithMessage("密码长度不能少于8位");
    }
}