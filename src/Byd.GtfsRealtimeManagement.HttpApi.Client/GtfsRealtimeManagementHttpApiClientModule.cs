using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Byd.GtfsRealtimeManagement;

[DependsOn(
    typeof(GtfsRealtimeManagementApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class GtfsRealtimeManagementHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(GtfsRealtimeManagementApplicationContractsModule).Assembly,
            GtfsRealtimeManagementRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<GtfsRealtimeManagementHttpApiClientModule>();
        });

    }
}
