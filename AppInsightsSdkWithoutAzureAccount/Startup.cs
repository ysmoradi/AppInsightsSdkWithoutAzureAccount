using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppInsightsSdkWithoutAzureAccount
{
    public class Startup
    {
        public IWebHostEnvironment WebHostEnvironment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            WebHostEnvironment = webHostEnvironment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
        }

        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    TelemetryClient telemetryClient = context.RequestServices.GetRequiredService<TelemetryClient>(); // You can inject TelemetryClient in controllers and services to provide additional data to app insights sdk.

                    using (IOperationHolder<DependencyTelemetry> exportExcelOperation = telemetryClient.StartOperation<DependencyTelemetry>("ExportToExcel"))
                    {
                        exportExcelOperation.Telemetry.Type = "Background";

                        try
                        {
                            await Task.Delay(1000);
                            telemetryClient.TrackTrace($"done {30}%");
                            await Task.Delay(1000);
                            telemetryClient.TrackTrace($"done {60}%");
                            await Task.Delay(1000);
                            telemetryClient.TrackTrace($"done {100}%");
                        }
                        catch (Exception exp)
                        {
                            telemetryClient.TrackException(exp);
                            throw;
                        }
                        finally
                        {
                            telemetryClient.StopOperation(exportExcelOperation);
                        }
                    }

                    telemetryClient.TrackEvent("MyEvent", new Dictionary<string, string>
                    {
                        { "prop1", "1" },
                        { "prop2", "2" }
                    });

                    ILogger<ClaimsIdentity> logger = context.RequestServices.GetRequiredService<ILogger<ClaimsIdentity>>(); // 3rd party libraries which relies on MS.Ext.Logging will also be captured by app insights sdk.

                    logger.LogWarning("User {user} was logged in...", "admin");

                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
