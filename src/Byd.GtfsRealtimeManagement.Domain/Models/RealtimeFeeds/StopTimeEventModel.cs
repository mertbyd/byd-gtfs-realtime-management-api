namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TripUpdate.StopTimeEvent'in aynasidir: bir varis ya da kalkisin gecikmesi ve zamani.
// sistemdeki gorevi: Kaynagin bir durak icin bildirdigi tahmini ya da gerceklesen zamani tasir.
public class StopTimeEventModel
{
    // Statik plana gore gecikme (sn); eksi deger erken demektir.
    public int? Delay { get; set; }

    // Mutlak zaman, POSIX saniye (UTC); Delay ile birlikte gelirse Time onceliklidir.
    public long? Time { get; set; }

    // Beklenen hata (sn); 0 kesin demektir.
    public int? Uncertainty { get; set; }

    // NEW, REPLACEMENT ve DUPLICATED seferin planlanan zamani, POSIX saniye (UTC).
    public long? ScheduledTime { get; set; }
}
