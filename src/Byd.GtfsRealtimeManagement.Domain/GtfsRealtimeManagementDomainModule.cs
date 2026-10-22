using Byd.AuthManagement;
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

}
