namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: RealtimeSource HTTP yollarini tanimlar.
// sistemdeki gorevi: CRUD rotalarini kararli bir sozlesmede tutar.
public static class RealtimeSourceRoutes
{
    public const string Root = "api/gtfs-realtime-management/realtime-sources";
    public const string ById = "{id:guid}";
    public const string FilteredList = "filtered-list";
}
