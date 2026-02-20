using Avalonia;
using AvaloniaUtility.Services;
using DIAbstract;
using FluentValidation;
using GeneralEditor.Database;
using GeneralEditor.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GeneralEditor;

public static class StartUp
{
    extension(IHostBuilder builder)
    {
        public IHostBuilder ConfigureAvaloniaServices()
        {
            return builder.ConfigureServices((ctx, service) =>
            {
                service.UseAvaloniaCore<GeneralEditorMenuWindow>()
                    .UseGeneralEditorCore()
                    .UseGeneralEditorDb();
            });
        }
    }

    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddMultiSingleton<Application, GeneralEditorApp>()
                .AddSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseGeneralEditorCore()
        {
            return collection
                .AddValidatorsFromAssembly(typeof(StartUp).Assembly, includeInternalTypes: true);
        }
    }
}