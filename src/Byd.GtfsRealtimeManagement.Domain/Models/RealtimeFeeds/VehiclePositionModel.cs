using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT VehiclePosition'in aynasidir; gelmeyen istege bagli alan bos kalir.
// sistemdeki gorevi: Aracin konumunu, hizini, duraga gore durumunu ve hizmet ettigi seferi tasir.
public class VehiclePositionModel
{
    public TripDescriptorModel? Trip { get; set; }
    public VehicleDescriptorModel? Vehicle { get; set; }
    public PositionModel? Position { get; set; }

    // Mevcut duragin stop_times sirasi; anlami CurrentStatus'a gore belirlenir.
    public uint? CurrentStopSequence { get; set; }

    public string? StopId { get; set; }

    // VehicleStopStatus kodu (0 INCOMING_AT, 1 STOPPED_AT, 2 IN_TRANSIT_TO); VehicleStopStatus lookup'inin Code'u.
    // Gelmezse standarttaki varsayilan IN_TRANSIT_TO; CurrentStopSequence yoksa standart bu alani yok sayar.
    public int? CurrentStatus { get; set; }

    // Konumun olculdugu an, POSIX saniye (UTC); cekme zamani degildir.
    public ulong? Timestamp { get; set; }

    // CongestionLevel kodu: 0 UNKNOWN_CONGESTION_LEVEL, 1 RUNNING_SMOOTHLY, 2 STOP_AND_GO, 3 CONGESTION, 4 SEVERE_CONGESTION.
    public int? CongestionLevel { get; set; }

    // OccupancyStatus kodu: 0 EMPTY ... 8 NOT_BOARDABLE.
    public int? OccupancyStatus { get; set; }

    // Doluluk yuzdesi.
    public uint? OccupancyPercentage { get; set; }

    public List<CarriageDetailsModel> MultiCarriageDetails { get; set; } = [];
}
