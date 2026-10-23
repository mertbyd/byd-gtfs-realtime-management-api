namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TimeRange'in aynasidir; uclardan biri bossa aralik o yonde aciktir.
// sistemdeki gorevi: Uyarinin gosterim ve etki araliklarini POSIX saniye (UTC) olarak tasir.
public class TimeRangeModel
{
    public ulong? Start { get; set; }
    public ulong? End { get; set; }
}
