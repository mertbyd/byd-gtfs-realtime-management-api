namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT EntitySelector'in aynasidir; dolu alanlar VE ile birlesir.
// sistemdeki gorevi: Uyarinin etkiledigi ajans, hat, hat turu, sefer, durak ve yonu tasir.
public class EntitySelectorModel
{
    public string? AgencyId { get; set; }
    public string? RouteId { get; set; }

    // GTFS route_type kodu.
    public int? RouteType { get; set; }

    public TripDescriptorModel? Trip { get; set; }
    public string? StopId { get; set; }
    public uint? DirectionId { get; set; }
}
