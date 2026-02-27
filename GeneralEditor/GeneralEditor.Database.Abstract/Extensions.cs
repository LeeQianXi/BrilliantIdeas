using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralEditor.Database.Abstract;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddGeneralEditorDtoValidators()
        {
            return services
                .AddValidatorsFromAssembly(typeof(Extensions).Assembly, includeInternalTypes: true);
        }
    }
}