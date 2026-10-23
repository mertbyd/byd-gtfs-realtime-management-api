using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.Extensions;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.Interfaces;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

[ConnectionStringName(GtfsRealtimeManagementDbProperties.ConnectionStringName)]
public class GtfsRealtimeManagementDbContext : BydDbContext<GtfsRealtimeManagementDbContext>, IGtfsRealtimeManagementDbContext
{
    public DbSet<TripScheduleRelationship> TripScheduleRelationships { get; set; } = default!;
    public DbSet<StopScheduleRelationship> StopScheduleRelationships { get; set; } = default!;
    public DbSet<VehicleStopStatus> VehicleStopStatuses { get; set; } = default!;
    public DbSet<ViolationType> ViolationTypes { get; set; } = default!;
    public DbSet<RealtimeSource> RealtimeSources { get; set; } = default!;

    public GtfsRealtimeManagementDbContext(DbContextOptions<GtfsRealtimeManagementDbContext> options)
        : base(options)
    {
    }

    // Aktif kaynak filtresinin durumu, sorgu calistigi anda ABP kapsamindan okunur.
    protected virtual bool IsActiveRealtimeSourceFilterEnabled =>
        DataFilter?.IsEnabled<ActiveRealtimeSourceFilter>() ?? false;

    // Kaynak sorgulari organizasyonun aktif kaynagi sinirina girer.
    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
        => typeof(TEntity) == typeof(RealtimeSource)
           || base.ShouldFilterEntity<TEntity>(entityType);

    // Aktiflik kisiti kapatilsa bile tenant ve organizasyon sinirlari ayri ifade olarak kalir.
    protected override Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>(
        ModelBuilder modelBuilder, EntityTypeBuilder<TEntity> entityTypeBuilder)
    {
        var expression = base.CreateFilterExpression(modelBuilder, entityTypeBuilder);
        if (typeof(TEntity) == typeof(RealtimeSource))
        {
            Expression<Func<TEntity, bool>> active = entity => !IsActiveRealtimeSourceFilterEnabled ||
                EF.Property<bool>(entity, nameof(RealtimeSource.IsActive));
            expression = expression is null ? active : QueryFilterExpressionHelper.CombineExpressions(expression, active);
        }
        return expression;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(GtfsRealtimeManagementDbContext).Assembly);

        builder.ConfigureGtfsRealtimeManagement();

        builder.ApplySnakeCaseNaming();
    }
}
