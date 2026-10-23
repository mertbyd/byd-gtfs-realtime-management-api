using Microsoft.EntityFrameworkCore;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

[ConnectionStringName(GtfsRealtimeManagementDbProperties.ConnectionStringName)]
public interface IGtfsRealtimeManagementDbContext : IEfCoreDbContext
{
    DbSet<TripScheduleRelationship> TripScheduleRelationships { get; }
    DbSet<StopScheduleRelationship> StopScheduleRelationships { get; }
    DbSet<VehicleStopStatus> VehicleStopStatuses { get; }
    DbSet<ViolationType> ViolationTypes { get; }
    DbSet<RealtimeSource> RealtimeSources { get; }
    DbSet<RealtimeStopTime> RealtimeStopTimes { get; }
}
