namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TripUpdate.StopTimeUpdate'in cevap aynasidir.
// sistemdeki gorevi: Bir durak icin varis/kalkis zamanini ve durak durumunu tasir.
public sealed class StopTimeUpdateDto
{
    // Foreign Keys
    public string? StopId { get; set; }

    // Table Columns / Data
    public uint? StopSequence { get; set; }
    public int? DepartureOccupancyStatus { get; set; }
    public int? ScheduleRelationship { get; set; }

    // Navigation Properties
    public StopTimeEventDto? Arrival { get; set; }
    public StopTimeEventDto? Departure { get; set; }
    public StopTimePropertiesDto? StopTimeProperties { get; set; }
}
