using System;
using System.Collections.Concurrent;
using Byd.GtfsRealtimeManagement.Models.RealtimeStreams;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Bir organizasyonun arac eslemesini, kayit istegi bekleyen araclarini, araclarin sefer ilerleyisini, TripUpdates
// durumlarini, istemciye son gonderilen sefer tahminlerini, akislarin sagligini, yapilmayan sefer takibini ve aktif plan
// kopyasini tasir.
// sistemdeki gorevi: Cekme iscisi ile olay handler'larinin ayni anda okuyup yazdigi bellek kopyasidir.
public class RealtimeOrganizationSnapshotModel
{
    // Akistaki GTFS-RT arac kimligi ile arac modulundeki arac kimligi.
    public ConcurrentDictionary<string, Guid> VehicleIds { get; } = new();

    // Kayit istegi gonderilmis, arac olayi henuz gelmemis GTFS-RT arac kimlikleri.
    public ConcurrentDictionary<string, byte> RequestedVehicleIds { get; } = new();

    // Akistaki GTFS-RT arac kimligi ile aracin seferindeki ilerleyisi.
    public ConcurrentDictionary<string, RealtimeVehicleVisitModel> VehicleVisits { get; } = new();

    // Sefer ornegi ve durak anahtari ile TripUpdates akisinin bildirdigi son durum.
    public ConcurrentDictionary<string, RealtimeStopStatusModel> StopStatuses { get; } = new();

    // Sefer ornegi anahtari ile istemciye son gonderilen sefer tahmini; yalniz degisen seferi gondermek icindir.
    public ConcurrentDictionary<string, RealtimeTripUpdateStreamModel> SentTripUpdates { get; } = new();

    // Sefer tahminlerinin son kez tam liste olarak gonderildigi an, UTC; bossa ilk cekmede tam liste gider.
    public DateTime? TripUpdatesFullDatasetSentAt { get; set; }

    // Akis turu ile son cekmedeki sagligi; yalniz degistiginde bildirim gider. Kayit yoksa akis saglikli sayilir.
    public ConcurrentDictionary<RealtimeFeedType, RealtimeFeedHealth> FeedHealth { get; } = new();

    // Akis turu ile son islenen baslik zamani; ayni baslik zamanli yanit yeniden islenmez.
    public ConcurrentDictionary<RealtimeFeedType, ulong> ProcessedHeaderTimestamps { get; } = new();

    // Aracla eslesmis sefer orneklerinin (sefer ve servis gunu) anahtari ile servis gunu; yapilmayan sefer bunlarin
    // disindadir.
    public ConcurrentDictionary<string, DateOnly> ObservedTrips { get; } = new();

    // Arac konumu akisinin kesintisiz saglikli islendigi baslangic ani, UTC; bu andan once baslayan sefer izlenmemis sayilir.
    public DateTime? WatchedSince { get; set; }

    // Yapilmayan sefer kontrolunun en son bakildigi an, UTC; suresi bu andan sonra dolan seferlere bakilir.
    public DateTime? MissedTripsCheckedUntil { get; set; }

    // Organizasyonun aktif GTFS feed plani; aktif feed yoksa bos.
    public RealtimePlanSnapshotModel? Plan { get; set; }
}
