using Byd.AuthManagement;
using Byd.GtfsRealtimeManagement.Interfaces;
using Volo.Abp.Data;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(GtfsRealtimeManagementDomainSharedModule),
    typeof(AuthManagementDomainModule)
)]
public class GtfsRealtimeManagementDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
        => Configure<AbpDataFilterOptions>(options =>
            options.DefaultStates[typeof(ActiveRealtimeSourceFilter)] = new DataFilterState(true));
}
