using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Managers.Lookups;
using Volo.Abp.Domain.Repositories;

namespace Byd.GtfsRealtimeManagement.Data.Lookups;

// islevi: Varsayilan ihlal turu satirlarini idempotent olarak ekler.
// sistemdeki gorevi: Realtime kayitlarinin baglanacagi ViolationType satirlarinin her ortamda bulunmasini saglar.
public class ViolationTypeDataSeeder(
    ViolationTypeManager manager,
    IRepository<ViolationType, int> repository)
    : BaseLookupDataSeeder<ViolationType, ViolationTypeManager>(manager, repository);
