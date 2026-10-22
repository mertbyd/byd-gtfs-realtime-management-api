using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: StopScheduleRelationship tablosunu ortak lookup EF kurallariyla esler.
// sistemdeki gorevi: Tablo ve kod indeksini modulun semasina baglar.
public sealed class StopScheduleRelationshipConfiguration()
    : RealtimeLookupConfiguration<StopScheduleRelationship>(StopScheduleRelationshipConsts.TableName, StopScheduleRelationshipConsts.CodeIndex);
