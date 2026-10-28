using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStreams;

// islevi: Aracin son konumunu, seferini, hattini ve seferdeki durak ilerleyisini tasir.
// sistemdeki gorevi: Her arac konumu cekmesinden sonra organizasyona giden canli akisin govdesine JSON olarak yazilir;
// harita butun listeyi, secili arac ekrani listeden kendi aracini kullanir.
public class RealtimeVehicleStreamModel
{
    // Arac modulundeki arac; arac henuz kayitli degilse bos.
    public Guid? VehicleId { get; set; }
    public string GtfsVehicleId { get; set; } = default!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Bearing { get; set; }

    // GTFS-RT Position.speed, metre/saniye.
    public double? Speed { get; set; }

    // Konumun olcum zamani, UTC.
    public DateTime? MeasuredAt { get; set; }

    // Sefer planda eslestiyse seferi, hatti ve servis gunu.
    public string? GtfsTripId { get; set; }
    public string? GtfsRouteId { get; set; }
    public DateOnly? ServiceDate { get; set; }

    // Aracin yaricapinda bulundugu durak ve orada durup durmadigi.
    public int? CurrentStopSequence { get; set; }
    public string? CurrentGtfsStopId { get; set; }
    public bool IsStopped { get; set; }

    // Seferde siradaki durak ve kaynagin o durak icin bildirdigi gecikme, saniye.
    public int? NextStopSequence { get; set; }
    public string? NextGtfsStopId { get; set; }
    public int? ReportedDelay { get; set; }
}
