namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT Position'in cevap aynasidir.
// sistemdeki gorevi: Aracin WGS-84 konumunu, yonunu, kilometre sayacini ve hizini (m/s) tasir.
public sealed class PositionDto
{
    // Table Columns / Data
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float? Bearing { get; set; }
    public double? Odometer { get; set; }
    public float? Speed { get; set; }
}
