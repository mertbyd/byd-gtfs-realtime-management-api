using Byd.AuthManagement;
using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace Byd.GtfsRealtimeManagement;

[DependsOn(
    typeof(GtfsRealtimeManagementDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule),
    typeof(AuthManagementApplicationContractsModule)
    )]
public class GtfsRealtimeManagementApplicationContractsModule : AbpModule
{

}
