namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TranslatedString.Translation'in cevap aynasidir.
// sistemdeki gorevi: Tek dildeki metni ve dil kodunu tasir.
public sealed class TranslationDto
{
    // Table Columns / Data
    public string Text { get; set; } = default!;
    public string? Language { get; set; }
}
