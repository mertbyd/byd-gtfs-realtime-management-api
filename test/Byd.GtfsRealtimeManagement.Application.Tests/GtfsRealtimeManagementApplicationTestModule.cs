using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement;

[DependsOn(
    typeof(GtfsRealtimeManagementApplicationModule),
    typeof(GtfsRealtimeManagementDomainTestModule)
    )]
public class GtfsRealtimeManagementApplicationTestModule : AbpModule
{
    // Cekme iscisi testlerde baslamaz; testlerin olusturdugu aktif kaynaklarin adreslerine istek atilmaz.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundWorkerOptions>(options => options.IsEnabled = false);
    }
}
