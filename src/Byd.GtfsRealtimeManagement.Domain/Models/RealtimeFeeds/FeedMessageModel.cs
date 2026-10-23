using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT FeedMessage'in alan ve tip olarak aynasidir: baslik ve akistaki varliklar.
// sistemdeki gorevi: Okuyucunun disari verdigi model; uretilen protobuf siniflari okuyucudan disari cikmaz.
public class FeedMessageModel
{
    public FeedHeaderModel Header { get; set; } = default!;
    public List<FeedEntityModel> Entity { get; set; } = [];
}
