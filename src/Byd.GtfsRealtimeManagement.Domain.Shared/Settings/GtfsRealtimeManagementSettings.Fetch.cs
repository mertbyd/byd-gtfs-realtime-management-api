namespace Byd.GtfsRealtimeManagement.Settings;

/// <summary>
/// GTFS-RT akislarinin cekme araligi ayar adlarini tanimlar.
/// </summary>
// islevi: Her akis turunun iki cekme arasindaki saniyesinin ayar adini tanimlar.
// sistemdeki gorevi: Cekme iscisi araligi bu adlarla kaynagin kapsaminda okur; organizasyon, tenant ve global seviyede
// degistirilebilir.
public partial class GtfsRealtimeManagementSettings
{
    public static class Fetch
    {
        public const string Default = GroupName + ".Fetch";

        // VehiclePositions akisinin iki cekme arasindaki saniyesi.
        public const string VehiclePositionsIntervalSeconds = Default + ".VehiclePositionsIntervalSeconds";

        // TripUpdates akisinin iki cekme arasindaki saniyesi.
        public const string TripUpdatesIntervalSeconds = Default + ".TripUpdatesIntervalSeconds";

        // ServiceAlerts akisinin iki cekme arasindaki saniyesi.
        public const string ServiceAlertsIntervalSeconds = Default + ".ServiceAlertsIntervalSeconds";
    }
}
