using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: Durak olcumu tablosunun FK ve tekillik kurallarini tanimlar.
// sistemdeki gorevi: Islenmis realtime veriyi kaynak ve lookup satirlarina baglar.
public sealed class RealtimeStopTimeConfiguration : IEntityTypeConfiguration<RealtimeStopTime>
{
    // Tablo, FK ve sorgu indekslerini EF modeline uygular.
    public void Configure(EntityTypeBuilder<RealtimeStopTime> builder)
    {
        builder.ToTable(RealtimeStopTimeConsts.TableName, GtfsRealtimeManagementDbProperties.DbSchema);
        builder.ConfigureByConvention();
        builder.HasOne(value => value.RealtimeSource).WithMany()
            .HasForeignKey(value => value.RealtimeSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(value => value.TripScheduleRelationship).WithMany()
            .HasForeignKey(value => value.TripScheduleRelationshipId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(value => value.StopScheduleRelationship).WithMany()
            .HasForeignKey(value => value.StopScheduleRelationshipId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(value => value.VehicleStopStatus).WithMany()
            .HasForeignKey(value => value.VehicleStopStatusId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(value => value.ViolationType).WithMany()
            .HasForeignKey(value => value.ViolationTypeId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(value => new { value.RealtimeSourceId, value.ServiceDate, value.GtfsTripId, value.StartTime, value.StopSequence })
            .IsUnique().AreNullsDistinct(false).HasDatabaseName(RealtimeStopTimeConsts.TripStopIndex);
        builder.HasIndex(value => new { value.RealtimeSourceId, value.ServiceDate, value.GtfsRouteId })
            .HasDatabaseName(RealtimeStopTimeConsts.RouteIndex);
        builder.HasIndex(value => new { value.RealtimeSourceId, value.ServiceDate, value.VehicleId })
            .HasDatabaseName(RealtimeStopTimeConsts.VehicleIndex);
        builder.HasIndex(value => new { value.RealtimeSourceId, value.ServiceDate, value.ViolationTypeId })
            .HasDatabaseName(RealtimeStopTimeConsts.ViolationIndex);
    }
}
