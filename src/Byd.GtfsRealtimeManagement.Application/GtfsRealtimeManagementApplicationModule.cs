using Microsoft.Extensions.DependencyInjection;
using Byd.LogManagement;
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
    typeof(LogManagementApplicationModule)
    )]
public class GtfsRealtimeManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<GtfsRealtimeManagementApplicationModule>();
    }
}
