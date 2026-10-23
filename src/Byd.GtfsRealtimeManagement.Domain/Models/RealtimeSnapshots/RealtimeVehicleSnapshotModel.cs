using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Arac modulundeki aracin kimligini ve akistaki GTFS-RT kimligini tasir.
// sistemdeki gorevi: Arac kopyasinin Dapper ile yuklenen satiridir.
public class RealtimeVehicleSnapshotModel
{
    public Guid VehicleId { get; set; }
    public string GtfsVehicleId { get; set; } = default!;
}
