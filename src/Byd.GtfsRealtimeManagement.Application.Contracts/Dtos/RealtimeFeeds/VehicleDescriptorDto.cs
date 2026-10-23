namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT VehicleDescriptor'in cevap aynasidir.
// sistemdeki gorevi: Akisin araci tanittigi kimligi, etiketi ve plakayi tasir.
public sealed class VehicleDescriptorDto
{
    // Primary Key
    public string? Id { get; set; }

    // Table Columns / Data
    public string? Label { get; set; }
    public string? LicensePlate { get; set; }
    public int? WheelchairAccessible { get; set; }
}
