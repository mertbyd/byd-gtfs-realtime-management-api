using System.Collections.Generic;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;

namespace Byd.GtfsRealtimeManagement.Managers.Lookups;

// islevi: VehicleStopStatus satirlarinin olusturulmasini ve varsayilan satirlarini ortak lookup kurallarina baglar.
// sistemdeki gorevi: VehicleStopStatus icin tipli entity kurulumunu ve seed tanimlarini saglar.
public class VehicleStopStatusManager(IRealtimeLookupRepository<VehicleStopStatus> repository)
    : BaseLookupManager<VehicleStopStatus>(repository)
{
    // Varsayilan arac durak durumu satirlarini Domain.Shared uzerinden alir.
    protected override IReadOnlyList<(int Id, string Name, int Code)> GetDefinitions()
        => VehicleStopStatusConsts.Definitions;

    // Tanimli kodu VehicleStopStatus entity'si olarak kurar.
    protected override VehicleStopStatus CreateEntity(int id, string name, int code)
        => new(id, name, code);
}
