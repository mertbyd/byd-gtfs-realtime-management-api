using System;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStopTimes;

// islevi: Bir cekmede bir sefer orneginin bir duragi icin bulunan giris, cikis, gecis ya da durum degisikligini tasir.
// sistemdeki gorevi: Isleme adimi degisiklikleri toplar ve tek is biriminde durak satirlarina yazar.
public class RealtimeStopTimeChangeModel
{
    public RealtimeTripInstanceModel Instance { get; set; } = default!;
    public RealtimeStopTimeSnapshotModel Stop { get; set; } = default!;
    public RealtimeStopTimeChangeKind Kind { get; set; }

    // Degisikligi kanitlayan cekmenin correlation kimligi.
    public string CorrelationId { get; set; } = default!;

    // Giris ve cikista aracin olcum zamani (UTC) ve aracin GTFS-RT kimligi.
    public DateTime? MeasuredAt { get; set; }
    public string? GtfsVehicleId { get; set; }

    // Giriste aracin konumu ve akisin bildirdigi durak durumu kodu.
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? VehicleStopStatusCode { get; set; }

    // Degisikligin satira yazdigi ihlal kodu: iptal edilen sefer, atlanan durak, erken ya da gec kalkis.
    public int? ViolationTypeCode { get; set; }
}
