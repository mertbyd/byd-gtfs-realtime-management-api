namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT EntitySelector'in cevap aynasidir; dolu alanlar VE ile birlesir.
// sistemdeki gorevi: Uyarinin etkiledigi ajans, hat, hat turu, sefer, durak ve yonu tasir.
public sealed class EntitySelectorDto
{
    // Foreign Keys
    // GTFS trips.txt direction_id kodu (0/1); GTFS'te yonun ayri bir adi yoktur.
    public uint? DirectionId { get; set; }
    public string? AgencyId { get; set; }
    public string? RouteId { get; set; }
    public string? StopId { get; set; }

    // Table Columns / Data
    public int? RouteType { get; set; }

    // Navigation Properties
    public TripDescriptorDto? Trip { get; set; }
}
