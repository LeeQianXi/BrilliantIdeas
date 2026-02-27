using System.Reflection;
using GeneralEditor.Core.Abstract.Services;
using Microsoft.Extensions.DependencyInjection;
using NetUtility;

namespace GeneralEditor.Core.Abstract;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddControllersFromAssemblyContaining<T>(bool includeInternalTypes = false)
            where T : class, IControlProvider
        {
            return services.AddControllerFromAssembly(typeof(T).Assembly, includeInternalTypes);
        }

        public IServiceCollection AddControllerFromAssembly(Assembly assembly, bool includeInternalTypes = false)
        {
            var defs = assembly.DefinedTypes
                .Where(t=>t.IsAssignableTo(typeof(IControlProvider)))
                .Where(t => t.HasAttribute<ControlProviderAttribute>());
            var sds = defs.Where(t => includeInternalTypes || t.IsPublic).Select(t =>
                new ServiceDescriptor(typeof(IControlProvider), t.AsType(), ServiceLifetime.Singleton));
            services.AddRange(sds);
            return services;
        }

        public IServiceCollection AddGeneralEditorControllers()
        {
            return services.AddControllerFromAssembly(typeof(Extensions).Assembly, true);
        }
    }
}