using System;
using System.Collections.Concurrent;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Bir organizasyonun arac eslemesini, kayit istegi bekleyen araclarini ve aktif plan kopyasini tasir.
// sistemdeki gorevi: Cekme iscisi ile olay handler'larinin ayni anda okuyup yazdigi bellek kopyasidir.
public class RealtimeOrganizationSnapshotModel
{
    // Akistaki GTFS-RT arac kimligi ile arac modulundeki arac kimligi.
    public ConcurrentDictionary<string, Guid> VehicleIds { get; } = new();

    // Kayit istegi gonderilmis, arac olayi henuz gelmemis GTFS-RT arac kimlikleri.
    public ConcurrentDictionary<string, byte> RequestedVehicleIds { get; } = new();

    // Organizasyonun aktif GTFS feed plani; aktif feed yoksa bos.
    public RealtimePlanSnapshotModel? Plan { get; set; }
}
