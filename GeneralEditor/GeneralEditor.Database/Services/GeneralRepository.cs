using GeneralEditor.Database.Abstract.Services;
using Microsoft.Extensions.Logging;

namespace GeneralEditor.Database.Services;

internal class GeneralRepository(
    ILogger<GeneralRepository> logger,
    GenDbContext dbContext
) : IGeneralRepository
{
}