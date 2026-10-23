namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT StopTimeUpdate.StopTimeProperties'in cevap aynasidir.
// sistemdeki gorevi: Duragin gercek zamanli degisen ozelliklerini tasir.
public sealed class StopTimePropertiesDto
{
    // Foreign Keys
    public string? AssignedStopId { get; set; }

    // Table Columns / Data
    public string? StopHeadsign { get; set; }
    public int? PickupType { get; set; }
    public int? DropOffType { get; set; }
}
