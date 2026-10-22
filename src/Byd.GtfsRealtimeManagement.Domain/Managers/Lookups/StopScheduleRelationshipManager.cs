using System.Collections.Generic;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;

namespace Byd.GtfsRealtimeManagement.Managers.Lookups;

// islevi: StopScheduleRelationship satirlarinin olusturulmasini ve varsayilan satirlarini ortak lookup kurallarina baglar.
// sistemdeki gorevi: StopScheduleRelationship icin tipli entity kurulumunu ve seed tanimlarini saglar.
public class StopScheduleRelationshipManager(IRealtimeLookupRepository<StopScheduleRelationship> repository)
    : BaseLookupManager<StopScheduleRelationship>(repository)
{
    // Varsayilan durak planlama durumu satirlarini Domain.Shared uzerinden alir.
    protected override IReadOnlyList<(int Id, string Name, int Code)> GetDefinitions()
        => StopScheduleRelationshipConsts.Definitions;

    // Tanimli kodu StopScheduleRelationship entity'si olarak kurar.
    protected override StopScheduleRelationship CreateEntity(int id, string name, int code)
        => new(id, name, code);
}
