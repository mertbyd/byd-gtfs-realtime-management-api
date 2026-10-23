namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TripUpdate.StopTimeEvent'in cevap aynasidir.
// sistemdeki gorevi: Bir varis ya da kalkisin gecikmesini (sn) ve mutlak zamanini (POSIX saniye) tasir.
public sealed class StopTimeEventDto
{
    // Table Columns / Data
    public int? Delay { get; set; }
    public long? Time { get; set; }
    public int? Uncertainty { get; set; }
    public long? ScheduledTime { get; set; }
}
