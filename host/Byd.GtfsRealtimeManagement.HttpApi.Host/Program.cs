using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Byd.CoreExtensions.SystemLogs;
using Serilog;

namespace Byd.GtfsRealtimeManagement;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Ortak log yapilandirmasi; sistem log iscisi ayarlar degisince bu logger'i Elasticsearch hedefiyle yeniden kurar. Bu
        // yuzden CreateBootstrapLogger ile kurulur ve UseSerilog'a parametresiz baglanir.
        Log.Logger = new LoggerConfiguration()
            .ConfigureSystemLog(builder.Environment.EnvironmentName)
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting web host.");
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            await builder.AddApplicationAsync<GtfsRealtimeManagementHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
