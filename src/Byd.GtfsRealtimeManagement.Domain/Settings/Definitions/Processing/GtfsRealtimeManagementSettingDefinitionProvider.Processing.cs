using Volo.Abp.Settings;

namespace Byd.GtfsRealtimeManagement.Settings;

// islevi: Isleme adiminin durak, bayatlik, erken/gec kalkis ve yapilmayan sefer esiklerini ABP ayar tanimlarina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir.
public partial class GtfsRealtimeManagementSettingDefinitionProvider
{
    private static void AddProcessingSettings(ISettingDefinitionContext context)
    {
        // 50 m ve 1,5 m/s tasarim dokumanindaki Vilnius olcumlerinden. Bayatlik GTFS-RT best practice'ten: konum ve sefer
        // guncellemesi 90 sn'den, uyari 10 dk'dan eski olmamali. Kalkis toleranslari toplu tasimada yaygin "zamaninda"
        // penceresi: 1 dk erken, 5 dk gec. Yapilmayan sefer icin 60 dk, aracin sefere eslenebildigi pencerenin sonudur.
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
                defaultValue: "90",
                isVisibleToClients: false),
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Processing.ServiceAlertsStaleFeedSeconds,
                defaultValue: "600",
                isVisibleToClients: false),
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Processing.EarlyDepartureToleranceSeconds,
                defaultValue: "60",
                isVisibleToClients: false),
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Processing.LateDepartureToleranceSeconds,
                defaultValue: "300",
                isVisibleToClients: false),
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Processing.MissedTripGraceMinutes,
                defaultValue: "60",
                isVisibleToClients: false));
    }
}
