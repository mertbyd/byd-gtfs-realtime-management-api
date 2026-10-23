using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TripUpdate'in cevap aynasidir.
// sistemdeki gorevi: Sefer orneginin durumunu ve durak bazinda varis/kalkis guncellemelerini tasir.
public sealed class TripUpdateDto
{
    // Table Columns / Data
    public ulong? Timestamp { get; set; }
    public int? Delay { get; set; }

    // Navigation Properties
    public TripDescriptorDto Trip { get; set; } = default!;
    public VehicleDescriptorDto? Vehicle { get; set; }
    public List<StopTimeUpdateDto> StopTimeUpdate { get; set; } = [];
    public TripPropertiesDto? TripProperties { get; set; }
}
