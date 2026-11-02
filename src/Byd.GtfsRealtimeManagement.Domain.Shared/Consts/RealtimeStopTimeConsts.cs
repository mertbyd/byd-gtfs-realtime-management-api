namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: RealtimeStopTime tablo ve indeks adlarini ve satirlarin saklanma suresini tanimlar.
// sistemdeki gorevi: EF modelinin kalici ad sozlesmesini ve saklama iscisinin kuralini sabitler.
public static class RealtimeStopTimeConsts
{
    public const string TableName = "RealtimeStopTimes";
    public const string TripStopIndex = "IX_RealtimeStopTimes_Source_Service_Trip_Start_Sequence";
    public const string RouteIndex = "IX_RealtimeStopTimes_Source_Service_Route";
    public const string VehicleIndex = "IX_RealtimeStopTimes_Source_Service_Vehicle";
    public const string ViolationIndex = "IX_RealtimeStopTimes_Source_Service_Violation";

    // Servis gunu bu kadar gunden eski satirlar silinir; ham kanit log modulunde ayri saklanir.
    public const int RetentionDays = 90;

    // Saklama iscisinin tur araligi ve birden fazla instance'ta tek calismasini saglayan kilit.
    public const int RetentionScanPeriodSeconds = 3600;
    public const string RetentionLockName = "GtfsRealtimeManagement.RealtimeStopTimeRetention";
}
