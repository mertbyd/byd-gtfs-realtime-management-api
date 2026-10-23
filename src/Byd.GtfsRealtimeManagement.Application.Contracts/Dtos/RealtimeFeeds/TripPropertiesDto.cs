namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TripUpdate.TripProperties'in cevap aynasidir.
// sistemdeki gorevi: DUPLICATED, NEW ve REPLACEMENT seferin kimligini, tarihini ve saatini tasir.
public sealed class TripPropertiesDto
{
    // Foreign Keys
    public string? TripId { get; set; }
    public string? ShapeId { get; set; }

    // Table Columns / Data
    public string? StartDate { get; set; }
    public string? StartTime { get; set; }
    public string? TripHeadsign { get; set; }
    public string? TripShortName { get; set; }
}
