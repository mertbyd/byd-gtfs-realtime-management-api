namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: Realtime lookup HTTP yollarini tek noktada tanimlar.
// sistemdeki gorevi: Her lookup controller'inin rota sozlesmesini sabit tutar.
public static class RealtimeLookupRoutes
{
    public const string Root = "api/gtfs-realtime-management/lookups";
    public const string TripScheduleRelationships = "trip-schedule-relationships";
    public const string StopScheduleRelationships = "stop-schedule-relationships";
    public const string VehicleStopStatuses = "vehicle-stop-statuses";
    public const string ViolationTypes = "violation-types";
    public const string ById = "{id:int}";
    public const string TripScheduleRelationshipsPath = Root + "/" + TripScheduleRelationships;
    public const string StopScheduleRelationshipsPath = Root + "/" + StopScheduleRelationships;
    public const string VehicleStopStatusesPath = Root + "/" + VehicleStopStatuses;
    public const string ViolationTypesPath = Root + "/" + ViolationTypes;
}
