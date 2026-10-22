using Microsoft.Extensions.DependencyInjection;
using Byd.AuthManagement.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

[DependsOn(
    typeof(GtfsRealtimeManagementDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AuthManagementEntityFrameworkCoreModule)
)]
public class GtfsRealtimeManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<GtfsRealtimeManagementDbContext>(options => { options.AddDefaultRepositories(true); });
    }
}
