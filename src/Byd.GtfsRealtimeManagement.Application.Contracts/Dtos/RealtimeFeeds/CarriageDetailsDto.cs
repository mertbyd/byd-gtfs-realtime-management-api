namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT VehiclePosition.CarriageDetails'in cevap aynasidir.
// sistemdeki gorevi: Cok vagonlu aracta tek vagonun kimligini ve dolulugunu tasir.
public sealed class CarriageDetailsDto
{
    // Primary Key
    public string? Id { get; set; }

    // Table Columns / Data
    public string? Label { get; set; }
    public int? OccupancyStatus { get; set; }
    public int? OccupancyPercentage { get; set; }
    public uint? CarriageSequence { get; set; }
}
