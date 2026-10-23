namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: RealtimeSource HTTP yollarini tanimlar.
// sistemdeki gorevi: CRUD ve akis okuma rotalarini kararli bir sozlesmede tutar.
public static class RealtimeSourceRoutes
{
    public const string Root = "api/gtfs-realtime-management/realtime-sources";
    public const string ById = "{id:guid}";
    public const string IncludingInactive = ById + "/including-inactive";
    public const string Activate = ById + "/activate";
    public const string FilteredList = "filtered-list";
    public const string VehiclePositions = "vehicle-positions";
    public const string TripUpdates = "trip-updates";
    public const string ServiceAlerts = "service-alerts";
    public const string FeedMessage = "feed-message";
}
