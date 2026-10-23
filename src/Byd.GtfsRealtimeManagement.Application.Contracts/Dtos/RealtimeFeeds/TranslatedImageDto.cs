using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TranslatedImage'in cevap aynasidir.
// sistemdeki gorevi: Ayni gorselin dil basina adreslerini tasir.
public sealed class TranslatedImageDto
{
    // Navigation Properties
    public List<LocalizedImageDto> LocalizedImage { get; set; } = [];
}
