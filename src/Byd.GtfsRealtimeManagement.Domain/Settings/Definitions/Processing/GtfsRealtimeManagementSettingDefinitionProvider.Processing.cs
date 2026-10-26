using Volo.Abp.Settings;

namespace Byd.GtfsRealtimeManagement.Settings;

// islevi: Isleme adiminin durak ve bayatlik esiklerini ABP ayar tanimlarina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir.
public partial class GtfsRealtimeManagementSettingDefinitionProvider
{
    private static void AddProcessingSettings(ISettingDefinitionContext context)
    {
        // 50 m ve 1,5 m/s tasarim dokumanindaki Vilnius olcumlerinden; 120 sn, 10 sn'lik akisin 12 yenilemesi.
        context.Add(
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Processing.StopRadiusMeters,
                defaultValue: "50",
                isVisibleToClients: false),
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Processing.StoppedSpeedMetersPerSecond,
                defaultValue: "1.5",
                isVisibleToClients: false),
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Processing.StaleFeedSeconds,
                defaultValue: "120",
                isVisibleToClients: false));
    }
}
