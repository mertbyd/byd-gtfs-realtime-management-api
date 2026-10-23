namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;

// islevi: Kaynagin istemciden gelen duzenlenebilir alanlarini tasir.
// sistemdeki gorevi: Create ve update isteklerinin ortak sekil sozlesmesidir.
public abstract class RealtimeSourceInputDto
{
    // Table Columns / Data
    public string Name { get; set; } = default!;
    public string? VehiclePositionsUrl { get; set; }
    public string? TripUpdatesUrl { get; set; }
    public string? ServiceAlertsUrl { get; set; }
}
