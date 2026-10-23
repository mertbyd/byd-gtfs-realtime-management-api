using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.GtfsRealtimeManagement.Workers.RealtimeFeeds;
using Byd.LogManagement;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;

namespace Byd.GtfsRealtimeManagement;

// Auth'un Application katmani token sunucusunu tasidigi icin yerine log'un Application katmani baglanir.
[DependsOn(
    typeof(GtfsRealtimeManagementDomainModule),
    typeof(GtfsRealtimeManagementApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(LogManagementApplicationModule)
    )]
public class GtfsRealtimeManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<GtfsRealtimeManagementApplicationModule>();
    }

    // GTFS-RT cekme iscisini baslatir.
    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<RealtimeFeedFetchWorker>();
    }
}
