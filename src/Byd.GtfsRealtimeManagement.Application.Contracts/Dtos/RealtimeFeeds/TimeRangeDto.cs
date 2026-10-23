namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TimeRange'in cevap aynasidir.
// sistemdeki gorevi: Uyarinin gosterim ve etki araligini POSIX saniye olarak tasir; bos uc acik araliktir.
public sealed class TimeRangeDto
{
    // Table Columns / Data
    public ulong? Start { get; set; }
    public ulong? End { get; set; }
}
