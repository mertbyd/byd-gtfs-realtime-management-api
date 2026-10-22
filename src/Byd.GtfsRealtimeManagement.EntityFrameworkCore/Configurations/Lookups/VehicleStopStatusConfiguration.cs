using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: VehicleStopStatus tablosunu ortak lookup EF kurallariyla esler.
// sistemdeki gorevi: Tablo ve kod indeksini modulun semasina baglar.
public sealed class VehicleStopStatusConfiguration()
    : RealtimeLookupConfiguration<VehicleStopStatus>(VehicleStopStatusConsts.TableName, VehicleStopStatusConsts.CodeIndex);
