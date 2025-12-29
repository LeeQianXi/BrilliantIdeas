using AvaloniaUtility.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Client.Abstract;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Client.Abstract.Views;
using MultiPanel.Client.ViewModels;
using MultiPanel.Client.Views;
using MultiPanel.Interfaces;

namespace MultiPanel.Client;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddValidatorsFromAssemblyContaining<LoginInValidator>(includeInternalTypes: true)
                .AddSingleton<MultiPanelApp>()
                .AddSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseMultiPanelClient(IConfiguration configuration)
        {
            return collection
                    .UseClientServices()
                    .UseMultiPanelOrleansServices(configuration)
                    .AddSingleton<ILoginInViewModel, LoginInViewModel>()
                    .AddSingleton<IMainMenuView, MainMenuWindow>()
                    .AddSingleton<IMainMenuViewModel, MainMenuViewModel>()
                ;
        }
    }
}