namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TripDescriptor'in cevap aynasidir.
// sistemdeki gorevi: Arac konumu, sefer guncellemesi ve uyari kapsaminin ortak sefer ornegi tanimidir.
public sealed class TripDescriptorDto
{
    // Foreign Keys
    // GTFS trips.txt direction_id kodu (0/1); GTFS'te yonun ayri bir adi yoktur.
    public uint? DirectionId { get; set; }
    public string? TripId { get; set; }
    public string? RouteId { get; set; }

    // Table Columns / Data
    public string? StartTime { get; set; }
    public string? StartDate { get; set; }
    public int? ScheduleRelationship { get; set; }

    // Navigation Properties
    public ModifiedTripSelectorDto? ModifiedTrip { get; set; }
}
