namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TripDescriptor'in aynasidir; gelmeyen istege bagli alan bos kalir.
// sistemdeki gorevi: Arac konumu, sefer guncellemesi ve uyari kapsaminin ortak sefer ornegi tanimidir.
public class TripDescriptorModel
{
    public string? TripId { get; set; }
    public string? RouteId { get; set; }
    public uint? DirectionId { get; set; }

    // HH:MM:SS, 24:00 ustu olabilir; siklik tabanli seferde zorunlu.
    public string? StartTime { get; set; }

    // YYYYMMDD.
    public string? StartDate { get; set; }

    // TripDescriptor.ScheduleRelationship kodu (0 SCHEDULED, 1 ADDED, 2 UNSCHEDULED, 3 CANCELED, 5 REPLACEMENT,
    // 6 DUPLICATED, 7 DELETED, 8 NEW); TripScheduleRelationship lookup'inin Code'u.
    public int? ScheduleRelationship { get; set; }

    public ModifiedTripSelectorModel? ModifiedTrip { get; set; }
}
