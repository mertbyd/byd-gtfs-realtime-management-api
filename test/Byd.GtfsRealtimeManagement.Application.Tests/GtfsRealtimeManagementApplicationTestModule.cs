using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement;

[DependsOn(
    typeof(GtfsRealtimeManagementApplicationModule),
    typeof(GtfsRealtimeManagementDomainTestModule)
    )]
public class GtfsRealtimeManagementApplicationTestModule : AbpModule
{

}
