using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

namespace Byd.GtfsRealtimeManagement.Interfaces.RealtimeSnapshots;

// islevi: Organizasyonun araclarini ve aktif GTFS feed planini okuma sozlesmesini tanimlar.
// sistemdeki gorevi: Realtime kopyasinin veri kaynagidir; uygulamasi EF katmaninda Dapper ile yazilir.
public interface IRealtimeSnapshotRepository
{
    Task<List<RealtimeVehicleSnapshotModel>> GetVehiclesAsync(
        Guid tenantId, Guid organizationId, CancellationToken cancellationToken = default);

    Task<RealtimePlanSnapshotModel?> GetActivePlanAsync(
        Guid tenantId, Guid organizationId, CancellationToken cancellationToken = default);
}
