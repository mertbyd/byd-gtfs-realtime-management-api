using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Managers.Lookups;
using Volo.Abp.Domain.Repositories;

namespace Byd.GtfsRealtimeManagement.Data.Lookups;

// islevi: Varsayilan durak planlama durumu satirlarini idempotent olarak ekler.
// sistemdeki gorevi: Realtime kayitlarinin baglanacagi StopScheduleRelationship satirlarinin her ortamda bulunmasini saglar.
public class StopScheduleRelationshipDataSeeder(
    StopScheduleRelationshipManager manager,
    IRepository<StopScheduleRelationship, int> repository)
    : BaseLookupDataSeeder<StopScheduleRelationship, StopScheduleRelationshipManager>(manager, repository);
