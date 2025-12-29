using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiPanel.Abstractions.DTOs;
using MultiPanel.Client.Abstract.Bases;
using MultiPanel.Client.Abstract.ViewModels;

namespace MultiPanel.Client.ViewModels;

public class MainMenuViewModel(IServiceProvider serviceProvider) : ViewModelBase, IMainMenuViewModel
{
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<MainMenuViewModel>>();
    public AuthDto Auth { get; set; } = null!;
    public string UserName { get; set; } = string.Empty;
}