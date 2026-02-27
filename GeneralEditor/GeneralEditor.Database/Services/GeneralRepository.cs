using DIAbstract;
using GeneralEditor.Database.Abstract.Services;
using Microsoft.Extensions.Logging;

namespace GeneralEditor.Database.Services;

internal class GeneralRepository(
    ILogger<GeneralRepository> logger,
    GenDbContext dbContext
) : IGeneralRepository, IAsyncLifecycle
{
    public async Task InitializeAsync()
    {
        logger.LogInformation("Initializing GeneralRepository");
    }

    public async Task DisposeAsync()
    {
        logger.LogInformation("Disposing GeneralRepository");
    }
}