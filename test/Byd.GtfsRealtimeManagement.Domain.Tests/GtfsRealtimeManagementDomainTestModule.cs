using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement;

[DependsOn(
    typeof(GtfsRealtimeManagementDomainModule),
    typeof(GtfsRealtimeManagementTestBaseModule)
)]
public class GtfsRealtimeManagementDomainTestModule : AbpModule
{

}
