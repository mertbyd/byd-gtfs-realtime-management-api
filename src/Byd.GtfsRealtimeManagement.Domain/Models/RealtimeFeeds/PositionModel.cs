namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT Position'in aynasidir; enlem ve boylam zorunludur.
// sistemdeki gorevi: Aracin WGS-84 konumunu, yonunu, kilometre sayacini ve hizini tasir.
public class PositionModel
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }

    // Kuzeyden saat yonunde derece.
    public float? Bearing { get; set; }

    // Metre.
    public double? Odometer { get; set; }

    // Aracin olctugu anlik hiz (m/s).
    public float? Speed { get; set; }
}
