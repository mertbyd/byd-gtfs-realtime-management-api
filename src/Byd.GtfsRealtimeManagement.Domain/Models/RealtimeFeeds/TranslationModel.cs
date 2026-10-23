namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TranslatedString.Translation'in aynasidir.
// sistemdeki gorevi: Tek dildeki metni ve dil kodunu tasir.
public class TranslationModel
{
    // Zorunlu.
    public string Text { get; set; } = default!;

    // BCP-47 dil kodu; tek ceviri varsa bos olabilir.
    public string? Language { get; set; }
}
