using Volo.Abp.Settings;

namespace Byd.GtfsRealtimeManagement.Settings;

/// <summary>
/// Modulun ayarlarini ABP setting management'a tanitir.
/// </summary>
// islevi: Her ayar ailesinin kendi partial metodunu cagirir.
// sistemdeki gorevi: Yeni bir ayar ailesi eklemek bu dosyaya tek satir eklemekten ibarettir. Deger burada tutulmaz;
// varsayilan, global, tenant ve organizasyon degeri arasindan ABP'nin deger saglayici zinciri secer.
public partial class GtfsRealtimeManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        AddFetchSettings(context);
    }
}
