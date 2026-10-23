namespace Byd.GtfsRealtimeManagement.Models.RealtimeSources;

// islevi: Kaynagin duzenlenebilir alanlarini tasir.
// sistemdeki gorevi: Olusturma ve guncellemenin ayni domain kuralindan gecmesini saglar.
public abstract class RealtimeSourceInputModel
{
    public string Name { get; set; } = default!;
    public string? VehiclePositionsUrl { get; set; }
    public string? TripUpdatesUrl { get; set; }
    public string? ServiceAlertsUrl { get; set; }
}
