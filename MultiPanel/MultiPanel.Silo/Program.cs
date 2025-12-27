using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;

namespace MultiPanel.Silo;

public class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .UseOrleans(siloBuilder =>
            {
                siloBuilder.ConfigureLogging(logging => { logging.AddConsole(); })
                    .UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "multi-panel-cluster";
                        options.ServiceId = "MultiPanelSilo";
                    });
            })
            .RunConsoleAsync();
    }
}