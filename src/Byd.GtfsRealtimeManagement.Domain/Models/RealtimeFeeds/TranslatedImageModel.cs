using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TranslatedImage'in aynasidir.
// sistemdeki gorevi: Ayni gorselin dil basina adreslerini tasir.
public class TranslatedImageModel
{
    public List<LocalizedImageModel> LocalizedImage { get; set; } = [];
}
