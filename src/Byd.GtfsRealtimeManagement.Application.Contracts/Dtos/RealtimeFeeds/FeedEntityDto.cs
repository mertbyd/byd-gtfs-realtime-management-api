namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT FeedEntity'nin cevap aynasidir.
// sistemdeki gorevi: Akistaki tek varligi; sefer guncellemesi, arac konumu ya da uyariyi tasir.
public sealed class FeedEntityDto
{
    // Primary Key
    public string Id { get; set; } = default!;

    // Table Columns / Data
    public bool? IsDeleted { get; set; }

    // Navigation Properties
    public TripUpdateDto? TripUpdate { get; set; }
    public VehiclePositionDto? Vehicle { get; set; }
    public AlertDto? Alert { get; set; }
}
