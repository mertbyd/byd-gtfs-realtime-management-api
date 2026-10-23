using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TripUpdate'in aynasidir; gelmeyen istege bagli alan bos kalir.
// sistemdeki gorevi: Bir sefer orneginin durumunu ve durak bazinda varis/kalkis guncellemelerini tasir.
public class TripUpdateModel
{
    // Zorunlu; guncellemenin ait oldugu sefer ornegi.
    public TripDescriptorModel Trip { get; set; } = default!;

    public VehicleDescriptorModel? Vehicle { get; set; }
    public List<StopTimeUpdateModel> StopTimeUpdate { get; set; } = [];

    // Guncellemenin olculdugu an, POSIX saniye (UTC).
    public ulong? Timestamp { get; set; }

    // Seferin genel gecikmesi (sn); durak bazindaki gecikme StopTimeUpdate'tedir.
    public int? Delay { get; set; }

    public TripPropertiesModel? TripProperties { get; set; }
}
