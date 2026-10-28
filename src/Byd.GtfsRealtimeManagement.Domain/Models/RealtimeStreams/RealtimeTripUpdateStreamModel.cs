using System;
using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStreams;

// islevi: Plandaki bir sefer orneginin son durumunu, aracini, genel gecikmesini ve duraklarin tahmini zamanlarini tasir.
// sistemdeki gorevi: Her sefer guncellemesi cekmesinden sonra organizasyona giden canli akisin govdesine JSON olarak
// yazilir; istemci planlanan saatleri statik GTFS'ten okur, burada yalniz kimlikler ve akista degisen veri vardir.
public class RealtimeTripUpdateStreamModel
{
    // Arac modulundeki arac; akis arac bildirmediyse ya da arac henuz kayitli degilse bos.
    public Guid? VehicleId { get; set; }
    public string? GtfsVehicleId { get; set; }
    public string GtfsTripId { get; set; } = default!;
    public string GtfsRouteId { get; set; } = default!;
    public DateOnly ServiceDate { get; set; }

    // GTFS-RT start_time; ayni seferin farkli orneklerini ayirir.
    public string? StartTime { get; set; }

    // TripDescriptor.ScheduleRelationship kodu; iptal ve silinen seferde duraklar listelenmez, hepsi atlanmis sayilir.
    public int TripScheduleRelationshipCode { get; set; }

    // Seferin genel gecikmesi, saniye; kaynak bildirmediyse bos.
    public int? Delay { get; set; }

    // Sefer akistan dustu (GTFS-RT FeedEntity.is_deleted); yalniz degisenler listesinde gelir, duraklari bostur.
    public bool IsDeleted { get; set; }

    // Guncellemenin olculdugu an, UTC; guncelleme zaman bildirmediyse akis basligindaki an.
    public DateTime MeasuredAt { get; set; }

    // Kaynagin ilk bilgi verdigi duraktan son duraga kadar duraklar; oncesi icin tahmin yoktur.
    public List<RealtimeStopTimePredictionModel> Stops { get; set; } = [];
}
