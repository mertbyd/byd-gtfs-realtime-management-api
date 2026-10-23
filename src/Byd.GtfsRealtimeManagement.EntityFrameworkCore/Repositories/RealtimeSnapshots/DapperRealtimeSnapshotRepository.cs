using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories.Dapper;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: Arac modulunun arac kimliklerini ve GTFS modulunun aktif feed planini ortak veritabanindan Dapper ile okur.
// sistemdeki gorevi: Realtime kopyasinin tek veri kaynagidir. Moduller ayni veritabaninda kendi semalarindadir; bu
// repository yalniz okur, diger modullerin tablolarina yazmaz. SQL metinleri RealtimeSnapshotConsts'tadir.
[ExposeServices(typeof(IRealtimeSnapshotRepository))]
public class DapperRealtimeSnapshotRepository(IDbContextProvider<GtfsRealtimeManagementDbContext> dbContextProvider)
    : DapperRepository<GtfsRealtimeManagementDbContext>(dbContextProvider), IRealtimeSnapshotRepository,
        ITransientDependency
{
    // Organizasyonun araclarinin kimliklerini okur.
    public virtual async Task<List<RealtimeVehicleSnapshotModel>> GetVehiclesAsync(
        Guid tenantId, Guid organizationId, CancellationToken cancellationToken = default)
        => (await QueryAsync<RealtimeVehicleSnapshotModel>(RealtimeSnapshotConsts.VehiclesSql,
            new { tenantId, organizationId }, cancellationToken)).ToList();

    // Organizasyonun aktif feed'inin seferlerini, durak kimliklerini ve servis takvimlerini okur; aktif feed yoksa bos doner.
    public virtual async Task<RealtimePlanSnapshotModel?> GetActivePlanAsync(
        Guid tenantId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var feedId = (await QueryAsync<Guid>(RealtimeSnapshotConsts.ActiveFeedSql, new { tenantId, organizationId },
            cancellationToken)).Cast<Guid?>().FirstOrDefault();
        if (feedId is null)
            return null;

        return new RealtimePlanSnapshotModel
        {
            FeedId = feedId.Value,
            Trips = await GetTripsAsync(feedId.Value, cancellationToken),
            StopIds = (await QueryAsync<string>(RealtimeSnapshotConsts.StopIdsSql, new { feedId }, cancellationToken))
                .ToHashSet(),
            Services = await GetServicesAsync(feedId.Value, cancellationToken)
        };
    }

    // Feed'in seferlerini hat bilgisiyle okur ve her seferin duraklarini sirasiyla ekler.
    private async Task<Dictionary<string, RealtimeTripSnapshotModel>> GetTripsAsync(
        Guid feedId, CancellationToken cancellationToken = default)
    {
        var trips = (await QueryAsync<RealtimeTripSnapshotModel>(RealtimeSnapshotConsts.TripsSql, new { feedId },
            cancellationToken)).ToDictionary(trip => trip.GtfsTripId);
        foreach (var stopTime in await QueryAsync<RealtimeStopTimeSnapshotModel>(RealtimeSnapshotConsts.StopTimesSql,
                     new { feedId }, cancellationToken))
            trips[stopTime.GtfsTripId].StopTimes.Add(stopTime);
        return trips;
    }

    // Feed'in servislerini haftalik takvimleriyle okur ve tarih istisnalarini servislerine ekler.
    private async Task<Dictionary<string, RealtimeServiceSnapshotModel>> GetServicesAsync(
        Guid feedId, CancellationToken cancellationToken = default)
    {
        var services = (await QueryAsync<RealtimeServiceSnapshotModel>(RealtimeSnapshotConsts.ServicesSql,
            new { feedId }, cancellationToken)).ToDictionary(service => service.GtfsServiceId);
        foreach (var calendarDate in await QueryAsync<RealtimeCalendarDateSnapshotModel>(
                     RealtimeSnapshotConsts.CalendarDatesSql, new { feedId }, cancellationToken))
            services[calendarDate.GtfsServiceId].Exceptions[calendarDate.Date] = calendarDate.ExceptionType;
        return services;
    }

    // Sorguyu is biriminin baglanti ve transaction'inda calistirir.
    private async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters, CancellationToken cancellationToken)
        => await (await GetDbConnectionAsync()).QueryAsync<T>(
            new CommandDefinition(sql, parameters, await GetDbTransactionAsync(), cancellationToken: cancellationToken));
}
