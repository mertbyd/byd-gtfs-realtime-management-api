namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT VehicleDescriptor'in aynasidir.
// sistemdeki gorevi: Akisin araci tanittigi kimligi, etiketi ve plakayi tasir.
public class VehicleDescriptorModel
{
    // Aracin sistem kimligi; arac modulundeki GtfsVehicleId ile eslesir.
    public string? Id { get; set; }

    public string? Label { get; set; }
    public string? LicensePlate { get; set; }

    // WheelchairAccessible kodu: 0 NO_VALUE, 1 UNKNOWN, 2 WHEELCHAIR_ACCESSIBLE, 3 WHEELCHAIR_INACCESSIBLE.
    public int? WheelchairAccessible { get; set; }
}
