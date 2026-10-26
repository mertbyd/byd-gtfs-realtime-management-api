using System;
using System.Collections.Concurrent;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Bir organizasyonun arac eslemesini, kayit istegi bekleyen araclarini, araclarin sefer ilerleyisini, TripUpdates
// durumlarini ve aktif plan kopyasini tasir.
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

    // Organizasyonun aktif GTFS feed plani; aktif feed yoksa bos.
    public RealtimePlanSnapshotModel? Plan { get; set; }
}
