using Microsoft.EntityFrameworkCore;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
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
}