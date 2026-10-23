using Microsoft.Extensions.DependencyInjection;
using Byd.AuthManagement.EntityFrameworkCore;
using Byd.GtfsRealtimeManagement.Adapters.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.LogManagement;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: Realtime EF Core katmanini, varsayilan repository'leri ve GTFS-RT okuyucusunu kaydeder.
// sistemdeki gorevi: Okuyucunun dis servis kaydi icin log modulunun Application katmanina baglanir.
[DependsOn(
    typeof(GtfsRealtimeManagementDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AuthManagementEntityFrameworkCoreModule),
    typeof(LogManagementApplicationModule)
)]
public class GtfsRealtimeManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<GtfsRealtimeManagementDbContext>(options => { options.AddDefaultRepositories(true); });
        context.Services.AddTransient<IRealtimeFeedReader, RealtimeFeedReader>();
        context.Services.AddHttpClient(RealtimeFeedConsts.HttpClientName, client =>
            {
                client.Timeout = RealtimeFeedConsts.RequestTimeout;
                client.MaxResponseContentBufferSize = RealtimeFeedConsts.MaxResponseBytes;
            })
            .RemoveAllLoggers();
    }
}
