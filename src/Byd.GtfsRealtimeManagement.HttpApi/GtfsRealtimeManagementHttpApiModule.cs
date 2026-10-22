using Localization.Resources.AbpUi;
using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Byd.GtfsRealtimeManagement;

[DependsOn(
    typeof(GtfsRealtimeManagementApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class GtfsRealtimeManagementHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(GtfsRealtimeManagementHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<GtfsRealtimeManagementResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
