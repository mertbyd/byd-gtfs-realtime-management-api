namespace Byd.GtfsRealtimeManagement.RealtimeFeeds;

// GTFS-RT akis turu; kaynagin hangi adresinin ve hangi aralik ayarinin kullanilacagini secer, tabloda tutulmaz.
public enum RealtimeFeedType
{
    VehiclePositions = 1,
    TripUpdates = 2,
    ServiceAlerts = 3
}
