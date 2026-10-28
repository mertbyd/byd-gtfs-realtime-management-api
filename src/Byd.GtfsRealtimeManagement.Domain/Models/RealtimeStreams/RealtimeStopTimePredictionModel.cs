using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStreams;

// islevi: Seferin tek duragi icin durumu, tahmini varis ve kalkis anini ve bunlarin plana gore gecikmesini tasir.
// sistemdeki gorevi: Tahmin kaynagin bildirdigi zamandan, yoksa planlanan saate eklenen gecikmeden hesaplanir; gecikmesi
// bildirilmeyen durak onceki duragin gecikmesini devralir (GTFS-RT yayilim kurali).
public class RealtimeStopTimePredictionModel
{
    public int StopSequence { get; set; }
    public string? GtfsStopId { get; set; }

    // StopTimeUpdate.ScheduleRelationship kodu; atlanan ve verisi olmayan durakta tahmin bos kalir.
    public int StopScheduleRelationshipCode { get; set; }

    // Tahmini varis ve kalkis anlari, UTC.
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }

    // Plana gore gecikme, saniye; eksi deger erken demektir.
    public int? ArrivalDelay { get; set; }
    public int? DepartureDelay { get; set; }
}
