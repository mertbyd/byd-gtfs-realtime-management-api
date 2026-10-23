namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TranslatedImage.LocalizedImage'in aynasidir.
// sistemdeki gorevi: Tek dildeki gorselin adresini ve medya turunu tasir.
public class LocalizedImageModel
{
    // Zorunlu.
    public string Url { get; set; } = default!;

    // Zorunlu; IANA medya turu.
    public string MediaType { get; set; } = default!;

    public string? Language { get; set; }
}
