using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: TripScheduleRelationship tablosunu ortak lookup EF kurallariyla esler.
// sistemdeki gorevi: Tablo ve kod indeksini modulun semasina baglar.
public sealed class TripScheduleRelationshipConfiguration()
    : RealtimeLookupConfiguration<TripScheduleRelationship>(TripScheduleRelationshipConsts.TableName, TripScheduleRelationshipConsts.CodeIndex);
