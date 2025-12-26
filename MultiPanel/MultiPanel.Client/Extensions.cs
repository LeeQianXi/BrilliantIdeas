using AvaloniaUtility.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Client.ViewModels;
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

        public IServiceCollection UseMultiPanelClient()
        {
            return collection
                    .UseMultiPanelOrleansServices()
                    .AddSingleton<ILoginInViewModel, LoginInViewModel>()
                ;
        }
    }
}