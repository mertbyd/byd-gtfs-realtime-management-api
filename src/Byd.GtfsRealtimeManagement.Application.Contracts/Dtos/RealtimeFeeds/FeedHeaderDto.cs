namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT FeedHeader'in cevap aynasidir.
// sistemdeki gorevi: Akisin surumunu, artimlilik turunu ve uretildigi ani (POSIX saniye) tasir.
public sealed class FeedHeaderDto
{
    // Table Columns / Data
    public string GtfsRealtimeVersion { get; set; } = default!;
    public int? Incrementality { get; set; }
    public ulong? Timestamp { get; set; }
    public string? FeedVersion { get; set; }
}
