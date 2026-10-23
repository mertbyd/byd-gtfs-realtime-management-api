namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT FeedHeader'in aynasidir; gelmeyen istege bagli alan bos kalir.
// sistemdeki gorevi: Akisin surumunu, artimlilik turunu ve uretildigi ani tasir.
public class FeedHeaderModel
{
    // Zorunlu; "1.0" ya da "2.0".
    public string GtfsRealtimeVersion { get; set; } = default!;

    // FeedHeader.Incrementality kodu: 0 FULL_DATASET, 1 DIFFERENTIAL; gelmezse standarttaki varsayilan FULL_DATASET.
    public int? Incrementality { get; set; }

    // Icerigin olustugu an, POSIX saniye (UTC).
    public ulong? Timestamp { get; set; }

    // Statik feed_info.feed_version ile eslesen surum.
    public string? FeedVersion { get; set; }
}
