using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.Extensions;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Interfaces;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

[ConnectionStringName(GtfsRealtimeManagementDbProperties.ConnectionStringName)]
public class GtfsRealtimeManagementDbContext : BydDbContext<GtfsRealtimeManagementDbContext>, IGtfsRealtimeManagementDbContext
{
    public DbSet<TripScheduleRelationship> TripScheduleRelationships { get; set; } = default!;
    public DbSet<StopScheduleRelationship> StopScheduleRelationships { get; set; } = default!;
    public DbSet<VehicleStopStatus> VehicleStopStatuses { get; set; } = default!;
    public DbSet<ViolationType> ViolationTypes { get; set; } = default!;
    public DbSet<RealtimeSource> RealtimeSources { get; set; } = default!;
    public DbSet<RealtimeStopTime> RealtimeStopTimes { get; set; } = default!;

    public GtfsRealtimeManagementDbContext(DbContextOptions<GtfsRealtimeManagementDbContext> options)
        : base(options)
    {
    }

    // Aktif kaynak filtresinin durumu, sorgu calistigi anda ABP kapsamindan okunur.
    protected virtual bool IsActiveRealtimeSourceFilterEnabled =>
        DataFilter?.IsEnabled<ActiveRealtimeSourceFilter>() ?? false;

    // Alt satir kapsaminda tenant her zaman kaynaktan okunur.
    protected virtual Guid? FilterTenantId => LazyServiceProvider?.LazyGetRequiredService<ICurrentTenant>().Id;

    // Kaynak sorgulari organizasyonun aktif kaynagi sinirina girer.
    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
        => typeof(TEntity) == typeof(RealtimeSource)
           || typeof(IRealtimeSourceEntity).IsAssignableFrom(typeof(TEntity))
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
        if (typeof(IRealtimeSourceEntity).IsAssignableFrom(typeof(TEntity)))
            expression = AddSourceFilters(expression);
        return expression;
    }

    // Aktiflik kapatilsa da tenant ve organizasyon kaynagi sinirlamaya devam eder.
    private Expression<Func<TEntity, bool>> AddSourceFilters<TEntity>(Expression<Func<TEntity, bool>>? expression)
        where TEntity : class
    {
        Expression<Func<TEntity, bool>> scoped = entity => RealtimeSources.Any(source =>
            source.Id == EF.Property<Guid>(entity, nameof(IRealtimeSourceEntity.RealtimeSourceId))
            && source.TenantId == FilterTenantId
            && source.OrganizationId == CurrentOrganizationId
            && (!IsActiveRealtimeSourceFilterEnabled || source.IsActive));
        return expression is null ? scoped : QueryFilterExpressionHelper.CombineExpressions(expression, scoped);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(GtfsRealtimeManagementDbContext).Assembly);

        builder.ConfigureGtfsRealtimeManagement();

        builder.ApplySnakeCaseNaming();
    }
}
