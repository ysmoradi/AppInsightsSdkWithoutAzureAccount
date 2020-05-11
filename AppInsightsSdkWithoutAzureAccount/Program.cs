using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.EventFlow;
using Microsoft.Diagnostics.EventFlow.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppInsightsSdkWithoutAzureAccount
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (var pipeline = DiagnosticPipelineFactory.CreatePipeline("eventFlowConfig.json"))
            {
                await CreateHostBuilder(args, pipeline)
                    .Build()
                    .RunAsync();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, DiagnosticPipeline pipeline) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services.AddSingleton<ITelemetryProcessorFactory>(sp => new EventFlowTelemetryProcessorFactory(pipeline)))
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
