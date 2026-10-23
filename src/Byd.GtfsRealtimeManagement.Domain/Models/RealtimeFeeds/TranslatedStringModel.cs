using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TranslatedString'in aynasidir.
// sistemdeki gorevi: Ayni metnin dil basina cevirilerini tasir.
public class TranslatedStringModel
{
    public List<TranslationModel> Translation { get; set; } = [];
}
