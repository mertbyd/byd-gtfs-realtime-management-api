using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.Querying;
using Byd.CoreExtensions.Repositories;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeStopTimes;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: Durak satirlarini lookup adlariyla birlikte getirir.
// sistemdeki gorevi: Filtreli liste ve detay sorgusunu EF Core'da uygular.
[ExposeServices(typeof(IRealtimeStopTimeRepository))]
public class EfCoreRealtimeStopTimeRepository(
    IDbContextProvider<GtfsRealtimeManagementDbContext> dbContextProvider)
    : EfExtendedRepository<GtfsRealtimeManagementDbContext, RealtimeStopTime, Guid>(dbContextProvider),
        IRealtimeStopTimeRepository
{
    // Tek satiri adlariyla birlikte getirir; global kaynak filtresi sorguda kalir.
    public virtual async Task<RealtimeStopTime?> FindWithLookupsAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await WithLookups(await GetQueryableAsync()).FirstOrDefaultAsync(value => value.Id == id, cancellationToken);

    // Kaynagin servis gunundeki verilen seferlerinin satirlarini isleme adiminin guncellemesi icin izlenir sekilde getirir.
    public virtual async Task<List<RealtimeStopTime>> GetListForTripsAsync(Guid realtimeSourceId, DateOnly serviceDate,
        IReadOnlyCollection<string> gtfsTripIds, CancellationToken cancellationToken = default)
        => await (await GetQueryableAsync())
            .Where(value => value.RealtimeSourceId == realtimeSourceId && value.ServiceDate == serviceDate
                && gtfsTripIds.Contains(value.GtfsTripId))
            .ToListAsync(cancellationToken);

    // Core DataTable sorgusuna sayfa materialize edilmeden lookup iliskilerini ekler.
    protected override async Task<IQueryable<RealtimeStopTime>> CreateFilteredQueryAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty)
        => WithLookups(await base.CreateFilteredQueryAsync(pagingProperty));

    // Cevapta gosterilecek kaynak ve lookup adlarini tek sorguda yukler.
    private static IQueryable<RealtimeStopTime> WithLookups(IQueryable<RealtimeStopTime> query)
        => query.Include(value => value.RealtimeSource)
            .Include(value => value.TripScheduleRelationship)
            .Include(value => value.StopScheduleRelationship)
            .Include(value => value.VehicleStopStatus)
            .Include(value => value.ViolationType);
}
