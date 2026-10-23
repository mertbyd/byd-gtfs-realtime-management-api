namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: RealtimeStopTime tablo ve indeks adlarini tanimlar.
// sistemdeki gorevi: EF modelinin kalici ad sozlesmesini sabitler.
public static class RealtimeStopTimeConsts
{
    public const string TableName = "RealtimeStopTimes";
    public const string TripStopIndex = "IX_RealtimeStopTimes_Source_Service_Trip_Start_Sequence";
    public const string RouteIndex = "IX_RealtimeStopTimes_Source_Service_Route";
    public const string VehicleIndex = "IX_RealtimeStopTimes_Source_Service_Vehicle";
    public const string ViolationIndex = "IX_RealtimeStopTimes_Source_Service_Violation";
}
