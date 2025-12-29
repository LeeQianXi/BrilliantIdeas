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

    public Interaction<string, Unit> WarningInfo { get; } = new();

    public Interaction<IMainMenuView, Unit> SuccessLoginInteraction { get; } = new();

    public async Task<AuthDto?> LoginWithRememberMeAsync()
    {
        if (!RememberMe) return null;
        var client = ServiceProvider.GetRequiredService<IClusterClient>();
        var ag = client.GetGrain<IAccountGrain>(Username);
        return await ag.LoginAsync(PasswordHash);
    }


    [RelayCommand]
    private async Task Login()
    {
        var validator = ServiceProvider.GetRequiredService<IValidator<LoginInViewModel>>();
        var result = await validator.ValidateAsync(this);
        if (!result.IsValid)
        {
            var validateInfo = result.Errors.First();
            await WarningInfo.Handle(validateInfo.ErrorMessage);
            return;
        }

        Logger.LogInformation("Trying to login to server");
        var client = ServiceProvider.GetRequiredService<IClusterClient>();
        var ag = client.GetGrain<IAccountGrain>(Username);
        if (!await ag.ExistAsync())
        {
            await WarningInfo.Handle("Account not found");
            return;
        }

        var pwdHash = Password.Hash(SHA384.Create());
        PasswordHash = pwdHash;
        var auth = await ag.LoginAsync(pwdHash);
        if (!auth.IsValid)
        {
            await WarningInfo.Handle("Username or Password is invalid");
            return;
        }

        await SuccessLoginCommand.ExecuteAsync(auth);
    }

    [RelayCommand]
    private async Task Register()
    {
        var validator = ServiceProvider.GetRequiredService<IValidator<LoginInViewModel>>();
        var result = await validator.ValidateAsync(this);
        if (!result.IsValid)
        {
            var validateInfo = result.Errors.First();
            await WarningInfo.Handle(validateInfo.ErrorMessage);
            return;
        }

        Logger.LogInformation("Trying to register a new account");
        var client = ServiceProvider.GetRequiredService<IClusterClient>();
        var ag = client.GetGrain<IAccountGrain>(Username);
        if (await ag.ExistAsync())
        {
            await WarningInfo.Handle("Account is already registered");
            return;
        }

        var pwdHash = Password.Hash(SHA384.Create());
        PasswordHash = pwdHash;
        var auth = await ag.RegisterAsync(pwdHash);
        if (!auth.IsValid)
        {
            await WarningInfo.Handle("Failed to register account");
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
}

/// <summary>
///     <see cref="LoginInViewModel" />的登陆信息验证器
/// </summary>
internal class LoginInValidator : AbstractValidator<LoginInViewModel>
{
    public LoginInValidator()
    {
        RuleFor(vm => vm.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(vm => vm.Password).NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must have at least 8 characters");
    }
}