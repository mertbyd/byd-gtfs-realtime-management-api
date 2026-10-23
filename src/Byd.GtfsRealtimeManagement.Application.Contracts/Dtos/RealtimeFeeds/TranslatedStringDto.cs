using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TranslatedString'in cevap aynasidir.
// sistemdeki gorevi: Ayni metnin dil basina cevirilerini tasir.
public sealed class TranslatedStringDto
{
    // Navigation Properties
    public List<TranslationDto> Translation { get; set; } = [];
}
