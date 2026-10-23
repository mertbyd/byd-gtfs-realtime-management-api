namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TranslatedImage.LocalizedImage'in cevap aynasidir.
// sistemdeki gorevi: Tek dildeki gorselin adresini ve medya turunu tasir.
public sealed class LocalizedImageDto
{
    // Table Columns / Data
    public string Url { get; set; } = default!;
    public string MediaType { get; set; } = default!;
    public string? Language { get; set; }
}
