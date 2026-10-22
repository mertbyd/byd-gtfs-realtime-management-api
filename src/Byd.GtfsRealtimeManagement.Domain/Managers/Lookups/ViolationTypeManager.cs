using System.Collections.Generic;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;

namespace Byd.GtfsRealtimeManagement.Managers.Lookups;

// islevi: ViolationType satirlarinin olusturulmasini ve varsayilan satirlarini ortak lookup kurallarina baglar.
// sistemdeki gorevi: ViolationType icin tipli entity kurulumunu ve seed tanimlarini saglar.
public class ViolationTypeManager(IRealtimeLookupRepository<ViolationType> repository)
    : BaseLookupManager<ViolationType>(repository)
{
    // Varsayilan ihlal turu satirlarini Domain.Shared uzerinden alir.
    protected override IReadOnlyList<(int Id, string Name, int Code)> GetDefinitions()
        => ViolationTypeConsts.Definitions;

    // Tanimli kodu ViolationType entity'si olarak kurar.
    protected override ViolationType CreateEntity(int id, string name, int code)
        => new(id, name, code);
}
