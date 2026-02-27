using DIAbstract;
using GeneralEditor.Database.Abstract.Services;
using GeneralEditor.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralEditor.Database;

public static class Extensions
{
    public static void ApplySqlLite(this DbContextOptionsBuilder builder)
    {
        Console.WriteLine(GenDbContext.DbPath);
        builder.UseSqlite($"Data Source={GenDbContext.DbPath}");
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection UseGeneralEditorDb()
        {
            services.AddPooledDbContextFactory<GenDbContext>(ApplySqlLite);
            services.AddMultiSingleton<IGeneralRepository, IAsyncLifecycle, GeneralRepository>();
            return services;
        }
    }
}