namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TripUpdate.TripProperties'in aynasidir.
// sistemdeki gorevi: DUPLICATED, NEW ve REPLACEMENT seferin kimligini, tarihini ve saatini tasir.
public class TripPropertiesModel
{
    public string? TripId { get; set; }

    // YYYYMMDD.
    public string? StartDate { get; set; }

    // HH:MM:SS, 24:00 ustu olabilir.
    public string? StartTime { get; set; }

    public string? ShapeId { get; set; }
    public string? TripHeadsign { get; set; }
    public string? TripShortName { get; set; }
}
