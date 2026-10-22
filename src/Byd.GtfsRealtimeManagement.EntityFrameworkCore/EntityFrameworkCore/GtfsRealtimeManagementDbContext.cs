using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.Extensions;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Volo.Abp.Data;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

[ConnectionStringName(GtfsRealtimeManagementDbProperties.ConnectionStringName)]
public class GtfsRealtimeManagementDbContext : BydDbContext<GtfsRealtimeManagementDbContext>, IGtfsRealtimeManagementDbContext
{
    public DbSet<TripScheduleRelationship> TripScheduleRelationships { get; set; } = default!;
    public DbSet<StopScheduleRelationship> StopScheduleRelationships { get; set; } = default!;
    public DbSet<VehicleStopStatus> VehicleStopStatuses { get; set; } = default!;
    public DbSet<ViolationType> ViolationTypes { get; set; } = default!;

    public GtfsRealtimeManagementDbContext(DbContextOptions<GtfsRealtimeManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(GtfsRealtimeManagementDbContext).Assembly);

        builder.ConfigureGtfsRealtimeManagement();

        builder.ApplySnakeCaseNaming();
    }
}
