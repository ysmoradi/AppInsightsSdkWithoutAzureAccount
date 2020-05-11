using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppInsightsSdkWithoutAzureAccount
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .Build()
                .RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logginBuilder =>
                {
                    logginBuilder.ClearProviders();
                    // app insights will add itself to loggingProviders, when you call services.AddApplicationInsightsTelemetry in Startup.cs
                    // You can clear other providers to reduce pressure.
                    // You can configure eventFlowConfig.json to submit your logs to multiple outputs such as Elastic.
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
