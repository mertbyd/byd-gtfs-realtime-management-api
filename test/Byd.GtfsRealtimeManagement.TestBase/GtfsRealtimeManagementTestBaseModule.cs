using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Autofac;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement;

// Global IDataSeeder burada calistirilmaz: platform modullerinin seed'leri SQLite test veritabaninda olmayan
// core tablolarina yazar. Realtime lookup seed'leri kendi testlerinde dogrudan calistirilir.
[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(AbpAuthorizationModule),
    typeof(AbpGuidsModule)
)]
public class GtfsRealtimeManagementTestBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysAllowAuthorization();
    }
}
