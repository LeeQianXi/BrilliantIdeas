using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiPanel.Client.Abstract.Bases;
using MultiPanel.Client.Abstract.ViewModels;

namespace MultiPanel.Client.ViewModels;

public class LoginInViewModel(IServiceProvider serviceProvider) : ViewModelBase, ILoginInViewModel
{
    public override IServiceProvider ServiceProvider { get; } = serviceProvider;
    public override ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<LoginInViewModel>>();
}