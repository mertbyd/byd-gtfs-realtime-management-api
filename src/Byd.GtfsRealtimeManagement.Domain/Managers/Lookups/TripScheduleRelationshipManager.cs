using System.Collections.Generic;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;

namespace Byd.GtfsRealtimeManagement.Managers.Lookups;

// islevi: TripScheduleRelationship satirlarinin olusturulmasini ve varsayilan satirlarini ortak lookup kurallarina baglar.
// sistemdeki gorevi: TripScheduleRelationship icin tipli entity kurulumunu ve seed tanimlarini saglar.
public class TripScheduleRelationshipManager(IRealtimeLookupRepository<TripScheduleRelationship> repository)
    : BaseLookupManager<TripScheduleRelationship>(repository)
{
    // Varsayilan sefer planlama durumu satirlarini Domain.Shared uzerinden alir.
    protected override IReadOnlyList<(int Id, string Name, int Code)> GetDefinitions()
        => TripScheduleRelationshipConsts.Definitions;

    // Tanimli kodu TripScheduleRelationship entity'si olarak kurar.
    protected override TripScheduleRelationship CreateEntity(int id, string name, int code)
        => new(id, name, code);
}
