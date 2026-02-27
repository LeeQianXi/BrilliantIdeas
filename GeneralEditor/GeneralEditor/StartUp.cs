using Avalonia;
using AvaloniaUtility.Services;
using DIAbstract;
using FluentValidation;
using GeneralEditor.Core.Abstract;
using GeneralEditor.Core.Abstract.ViewModel;
using GeneralEditor.Database;
using GeneralEditor.Database.Abstract;
using GeneralEditor.ViewModel;
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
                service.UseBrilliantInitializer();
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
                .AddMultiSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseGeneralEditorCore()
        {
            collection
                .AddValidatorsFromAssembly(typeof(StartUp).Assembly, includeInternalTypes: true)
                .AddGeneralEditorDtoValidators();
            collection
                .AddGeneralEditorControllers();
            collection
                .AddSingleton<IGeneralEditorMenuViewModel, GeneralEditorMenuMenuViewModel>();
            return collection;
        }
    }
}