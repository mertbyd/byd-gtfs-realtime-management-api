using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Managers.Lookups;
using Volo.Abp.Domain.Repositories;

namespace Byd.GtfsRealtimeManagement.Data.Lookups;

// islevi: Varsayilan arac durak durumu satirlarini idempotent olarak ekler.
// sistemdeki gorevi: Realtime kayitlarinin baglanacagi VehicleStopStatus satirlarinin her ortamda bulunmasini saglar.
public class VehicleStopStatusDataSeeder(
    VehicleStopStatusManager manager,
    IRepository<VehicleStopStatus, int> repository)
    : BaseLookupDataSeeder<VehicleStopStatus, VehicleStopStatusManager>(manager, repository);
