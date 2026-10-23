using Volo.Abp.Settings;

namespace Byd.GtfsRealtimeManagement.Settings;

// islevi: GTFS-RT akislarinin cekme araligi ayarlarini ABP ayar tanimlarina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir.
public partial class GtfsRealtimeManagementSettingDefinitionProvider
{
    private static void AddFetchSettings(ISettingDefinitionContext context)
    {
        // Konum ve sefer guncellemesi kaynak yenilendikce cekilir (Vilnius 10 sn'de bir yenileniyor); uyarilar seyrek degisir.
        context.Add(
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Fetch.VehiclePositionsIntervalSeconds,
                defaultValue: "10",
                isVisibleToClients: false),
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Fetch.TripUpdatesIntervalSeconds,
                defaultValue: "10",
                isVisibleToClients: false),
            new SettingDefinition(
                GtfsRealtimeManagementSettings.Fetch.ServiceAlertsIntervalSeconds,
                defaultValue: "60",
                isVisibleToClients: false));
    }
}
