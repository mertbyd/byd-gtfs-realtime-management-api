using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Managers.Lookups;
using Byd.GtfsRealtimeManagement.Mappers.Lookups;

namespace Byd.GtfsRealtimeManagement.Services.Lookups;

// islevi: StopScheduleRelationship satirlarinin CRUD akisini ortak lookup tabanina baglar.
// sistemdeki gorevi: Tipli baglari bildirir; dogrulama, Manager, Mapperly ve kalici yazma tabanda kalir.
public class StopScheduleRelationshipAppService
    : RealtimeLookupAppService<StopScheduleRelationship, StopScheduleRelationshipManager, StopScheduleRelationshipMapper>,
        IStopScheduleRelationshipAppService
{
}
