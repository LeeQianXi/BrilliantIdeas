using System.Reactive;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiPanel.Abstractions.DTOs;
using MultiPanel.Client.Abstract.Bases;
using MultiPanel.Client.Abstract.Options;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Interfaces.IGrains;
using MultiPanel.Shared.Services;
using ReactiveUI;

namespace MultiPanel.Client.ViewModels;

public partial class LoginInViewModel : ViewModelBase, ILoginInViewModel
{
    public LoginInViewModel(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<LoginInViewModel>>();
        var options = serviceProvider.GetRequiredService<IOptionsSnapshot<LoginWithOptions>>();
        RememberMe = options.Value.RememberMe;
        Username = options.Value.Username;
        Token = options.Value.Token;
    }

    private string Token { get; set; }

    public override IServiceProvider ServiceProvider { get; }
    public override ILogger Logger { get; }

    public bool RememberMe { get; set; }
    public string Username { get; set; }

    public string Password
    {
        get => field ?? string.Empty;
        set;
    }

    public Interaction<string, Unit> WarningInfo { get; } = new();

    public async Task<AccountInfo> TryLoginWithLastedData()
    {
        if (!RememberMe) return AccountInfo.Empty;
        var tg = ServiceProvider.GetRequiredService<ITokenGenerator>();
        var client = ServiceLocator.Instance.ClientContext.Client;
        //client.GetGrain<ISessionGrain>();
        return AccountInfo.Empty;
    }

    [RelayCommand]
    private async Task Login()
    {
        var validator = ServiceProvider.GetRequiredService<IValidator<LoginInViewModel>>();
        var result = await validator.ValidateAsync(this);
        if (!result.IsValid)
        {
            var validateInfo = result.Errors.First();
            WarningInfo.Handle(validateInfo.ErrorMessage);
            return;
        }

        Logger.LogInformation("Trying to login to server");
        var client = ServiceLocator.Instance.ClientContext.Client;
        var ag = client.GetGrain<IAccountGrain>(Username);
        var ph = ServiceProvider.GetRequiredService<IPasswordHasher>();
        var acInfo = await ag.TryLogin(ph.Hash(Password));
        if (acInfo == AccountInfo.Empty)
        {
            WarningInfo.Handle("Username or Password is invalid");
            return;
        }

        var sg = client.GetGrain<ISessionGrain>(acInfo.UserId);
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