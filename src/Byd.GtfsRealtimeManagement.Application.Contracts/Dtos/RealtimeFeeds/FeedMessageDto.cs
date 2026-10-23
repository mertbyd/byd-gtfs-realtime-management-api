using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT FeedMessage'in cevap aynasidir: baslik ve akistaki varliklar.
// sistemdeki gorevi: Kaynaktan anlik cekilen akisi standarttaki yapisiyla istemciye dondurur.
public sealed class FeedMessageDto
{
    // Navigation Properties
    public FeedHeaderDto Header { get; set; } = default!;
    public List<FeedEntityDto> Entity { get; set; } = [];
}
