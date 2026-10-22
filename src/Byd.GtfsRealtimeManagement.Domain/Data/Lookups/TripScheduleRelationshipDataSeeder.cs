using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Managers.Lookups;
using Volo.Abp.Domain.Repositories;

namespace Byd.GtfsRealtimeManagement.Data.Lookups;

// islevi: Varsayilan sefer planlama durumu satirlarini idempotent olarak ekler.
// sistemdeki gorevi: Realtime kayitlarinin baglanacagi TripScheduleRelationship satirlarinin her ortamda bulunmasini saglar.
public class TripScheduleRelationshipDataSeeder(
    TripScheduleRelationshipManager manager,
    IRepository<TripScheduleRelationship, int> repository)
    : BaseLookupDataSeeder<TripScheduleRelationship, TripScheduleRelationshipManager>(manager, repository);
