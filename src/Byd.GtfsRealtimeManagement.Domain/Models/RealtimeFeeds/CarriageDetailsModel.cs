namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT VehiclePosition.CarriageDetails'in aynasidir.
// sistemdeki gorevi: Cok vagonlu aracta tek vagonun kimligini ve dolulugunu tasir.
public class CarriageDetailsModel
{
    public string? Id { get; set; }
    public string? Label { get; set; }

    // OccupancyStatus kodu; gelmezse standarttaki varsayilan NO_DATA_AVAILABLE.
    public int? OccupancyStatus { get; set; }

    // Doluluk yuzdesi; gelmezse standarttaki varsayilan -1 (veri yok).
    public int? OccupancyPercentage { get; set; }

    public uint? CarriageSequence { get; set; }
}
