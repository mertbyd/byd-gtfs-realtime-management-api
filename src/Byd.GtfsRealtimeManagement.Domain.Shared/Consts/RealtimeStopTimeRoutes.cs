namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: Durak olcumu HTTP rotalarini tanimlar.
// sistemdeki gorevi: CRUD ve gecmis okuma yollarini sabitler.
public static class RealtimeStopTimeRoutes
{
    public const string Root = "api/gtfs-realtime-management/realtime-stop-times";
    public const string ById = "{id:guid}";
    public const string FilteredList = "filtered-list";
    public const string ForSource = "realtime-sources/{realtimeSourceId:guid}/{id:guid}";
    public const string ListForSource = "realtime-sources/{realtimeSourceId:guid}/filtered-list";
}
