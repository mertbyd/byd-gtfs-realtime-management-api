using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Data.Lookups;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Services.RealtimeSources;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.RealtimeStopTimes;

// islevi: Arac konumundan giris/cikis/gecis ve sefer guncellemesinden iptal durumunun durak satirlarina yazildigini; sefer
// guncellemesinden durak tahminlerinin GTFS-RT kurallariyla hesaplanip tam liste ya da yalniz degisenler olarak dondugunu
// dogrular.
// sistemdeki gorevi: Plan kopyasi sahteyle verilir; satirlar gercek repository ile SQLite'a yazilir.
public class RealtimeStopTimeProcessingManagerTests : GtfsRealtimeManagementEntityFrameworkCoreTestBase
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid VehicleId = Guid.NewGuid();
    private readonly IRealtimeSnapshotRepository _snapshotRepository = Substitute.For<IRealtimeSnapshotRepository>();

    private RealtimeStopTimeProcessingManager Processing =>
        ServiceProvider.GetRequiredService<RealtimeStopTimeProcessingManager>();

    // Plan kopyasini uc durakli, her gun calisan tek seferle kurar; dagitik olay yolu sahtedir.
    protected override void AfterAddApplication(IServiceCollection services)
    {
        _snapshotRepository.GetVehiclesAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>())
            .Returns([new RealtimeVehicleSnapshotModel { VehicleId = VehicleId, GtfsVehicleId = "V1" }]);
        _snapshotRepository.GetActivePlanAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>()).Returns(Plan());
        services.Replace(ServiceDescriptor.Singleton(_snapshotRepository));
        services.Replace(ServiceDescriptor.Singleton(Substitute.For<IDistributedEventBus>()));
    }

    [Fact]
    public async Task VehiclePositions_WriteArrivalDepartureAndPassedStops()
    {
        var sourceId = await PrepareAsync();
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await Processing.ProcessVehiclePositionsAsync(sourceId, Position(now, 54.6800, 25.2700), "c-1");
        await Processing.ProcessVehiclePositionsAsync(sourceId, Position(now + 20, 54.6900, 25.2900), "c-2");

        var rows = (await RowsAsync()).ToDictionary(row => row.StopSequence);
        Assert.Equal("c-1", rows[1].ArrivalCorrelationId);
        Assert.Equal(VehicleId, rows[1].VehicleId);
        Assert.NotNull(rows[1].DepartureTime);
        Assert.Equal("c-2", rows[1].DepartureCorrelationId);
        Assert.Null(rows[2].ArrivalTime);
        Assert.Null(rows[2].DepartureTime);
        Assert.Equal("c-2", rows[2].DepartureCorrelationId);
        Assert.Equal("c-2", rows[3].ArrivalCorrelationId);
        Assert.Null(rows[3].DepartureTime);
        var delay = DateTimeOffset.FromUnixTimeSeconds(now + 20).UtcDateTime - DateTime.UtcNow.Date;
        Assert.Equal(delay.TotalSeconds > 300 ? ViolationId(ViolationTypeConsts.LateDepartureCode) : null,
            rows[1].ViolationTypeId);
    }

    [Fact]
    public async Task TripUpdates_WriteCanceledTripOnceAsSkippedStops()
    {
        var sourceId = await PrepareAsync();
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await Processing.ProcessTripUpdatesAsync(sourceId, Canceled(now), "c-1");
        await Processing.ProcessTripUpdatesAsync(sourceId, Canceled(now + 10), "c-2");

        var rows = await RowsAsync();
        Assert.Equal(3, rows.Count);
        var canceledId = TripScheduleRelationshipConsts.Definitions
            .Single(value => value.Code == TripScheduleRelationshipConsts.CanceledCode).Id;
        var skippedId = StopScheduleRelationshipConsts.Definitions
            .Single(value => value.Code == StopScheduleRelationshipConsts.SkippedCode).Id;
        Assert.All(rows, row => Assert.Equal((canceledId, skippedId, "c-1", ViolationId(ViolationTypeConsts.CanceledTripCode)),
            (row.TripScheduleRelationshipId, row.StopScheduleRelationshipId, row.StatusCorrelationId, row.ViolationTypeId)));
    }

    [Fact]
    public async Task TripUpdates_PropagateDelayAndPreferReportedTime()
    {
        var sourceId = await PrepareAsync();
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var noon = DateOnly.FromDateTime(DateTime.UtcNow).ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc);
        var reported = noon.AddHours(5);

        var stream = await Processing.ProcessTripUpdatesAsync(sourceId, Updates(now, Update("T2",
            new StopTimeUpdateModel { StopId = "B", Departure = new StopTimeEventModel { Delay = 120 } },
            new StopTimeUpdateModel
            {
                StopId = "D", Arrival = new StopTimeEventModel { Delay = 5, Time = ((DateTimeOffset)reported).ToUnixTimeSeconds() }
            })), "c-1");

        var stops = Assert.Single(stream.Entities).Stops;
        Assert.Equal([2, 3, 4, 5], stops.Select(stop => stop.StopSequence));
        Assert.Equal(noon.AddSeconds(120), stops[0].DepartureTime);
        Assert.Equal((noon.AddHours(2).AddSeconds(120), 120), (stops[1].ArrivalTime, stops[1].ArrivalDelay));
        Assert.Equal((reported, 3600), (stops[2].ArrivalTime, stops[2].ArrivalDelay));
    }

    [Fact]
    public async Task TripUpdates_SkippedStopKeepsDelay_NoDataStopsPrediction()
    {
        var sourceId = await PrepareAsync();
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var stream = await Processing.ProcessTripUpdatesAsync(sourceId, Updates(now, Update("T2",
            new StopTimeUpdateModel { StopSequence = 1, Departure = new StopTimeEventModel { Delay = 60 } },
            new StopTimeUpdateModel { StopSequence = 2, ScheduleRelationship = StopScheduleRelationshipConsts.SkippedCode },
            new StopTimeUpdateModel { StopSequence = 4, ScheduleRelationship = StopScheduleRelationshipConsts.NoDataCode })),
            "c-1");

        var stops = Assert.Single(stream.Entities).Stops.ToDictionary(stop => stop.StopSequence);
        Assert.Null(stops[2].ArrivalTime);
        Assert.Equal(60, stops[3].ArrivalDelay);
        Assert.Equal((StopScheduleRelationshipConsts.NoDataCode, null),
            (stops[5].StopScheduleRelationshipCode, stops[5].ArrivalDelay));
        var skipped = (await RowsAsync()).Single(row => row.StopSequence == 2);
        Assert.Equal(ViolationId(ViolationTypeConsts.SkippedStopCode), skipped.ViolationTypeId);
    }

    [Fact]
    public async Task TripUpdates_SendOnlyChangesUntilFullDatasetIsDue()
    {
        var sourceId = await PrepareAsync();
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var first = await Processing.ProcessTripUpdatesAsync(sourceId, Updates(now, Delayed("T1", 60), Delayed("T2", 60)), "c-1");
        var same = await Processing.ProcessTripUpdatesAsync(sourceId,
            Updates(now + 10, Delayed("T1", 60), Delayed("T2", 60)), "c-2");
        var changed = await Processing.ProcessTripUpdatesAsync(sourceId, Updates(now + 20, Delayed("T1", 90)), "c-3");

        Assert.Equal((true, 2), (first.IsFullDataset, first.Entities.Count));
        Assert.Equal((false, 0), (same.IsFullDataset, same.Entities.Count));
        Assert.False(changed.IsFullDataset);
        Assert.Equal([("T1", false), ("T2", true)],
            changed.Entities.Select(trip => (trip.GtfsTripId, trip.IsDeleted)).OrderBy(trip => trip.GtfsTripId));
    }

    // Lookup'lari seed eder ve test organizasyonunda aktif kaynak kurar.
    private async Task<Guid> PrepareAsync()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await ServiceProvider.GetRequiredService<TripScheduleRelationshipDataSeeder>().SeedAsync(new DataSeedContext());
            await ServiceProvider.GetRequiredService<StopScheduleRelationshipDataSeeder>().SeedAsync(new DataSeedContext());
            await ServiceProvider.GetRequiredService<VehicleStopStatusDataSeeder>().SeedAsync(new DataSeedContext());
            await ServiceProvider.GetRequiredService<ViolationTypeDataSeeder>().SeedAsync(new DataSeedContext());
        });
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var sources = ServiceProvider.GetRequiredService<IRealtimeSourceAppService>();
        var source = await WithUnitOfWorkAsync(() => sources.CreateAsync(new CreateRealtimeSourceDto
        {
            Name = "Vilnius", VehiclePositionsUrl = "https://example.org/positions.pb"
        }));
        return (await WithUnitOfWorkAsync(() => sources.ActivateAsync(source.Id))).Id;
    }

    private static int ViolationId(int code) => ViolationTypeConsts.Definitions.Single(value => value.Code == code).Id;

    private Task<List<RealtimeStopTime>> RowsAsync()
        => WithUnitOfWorkAsync(() => ServiceProvider.GetRequiredService<IRealtimeStopTimeRepository>().GetListAsync());

    // Duraklar 1 km arayla dizilidir; seferler UTC gunun tamaminda planlidir. T2'nin her duraginin planlanan saati vardir.
    private static RealtimePlanSnapshotModel Plan() => new()
    {
        FeedId = Guid.NewGuid(),
        Trips = new Dictionary<string, RealtimeTripSnapshotModel>
        {
            ["T1"] = new()
            {
                GtfsTripId = "T1", GtfsServiceId = "S1", GtfsRouteId = "R1", RouteType = 3, Timezone = "UTC",
                StopTimes =
                [
                    new() { GtfsTripId = "T1", StopSequence = 1, GtfsStopId = "A", Latitude = 54.6800, Longitude = 25.2700,
                        DepartureTime = "00:00:00" },
                    new() { GtfsTripId = "T1", StopSequence = 2, GtfsStopId = "B", Latitude = 54.6850, Longitude = 25.2800 },
                    new() { GtfsTripId = "T1", StopSequence = 3, GtfsStopId = "C", Latitude = 54.6900, Longitude = 25.2900,
                        ArrivalTime = "23:59:59" }
                ]
            },
            ["T2"] = new()
            {
                GtfsTripId = "T2", GtfsServiceId = "S1", GtfsRouteId = "R1", RouteType = 3, Timezone = "UTC",
                StopTimes =
                [
                    new() { GtfsTripId = "T2", StopSequence = 1, GtfsStopId = "A", DepartureTime = "00:00:00" },
                    new() { GtfsTripId = "T2", StopSequence = 2, GtfsStopId = "B", ArrivalTime = "12:00:00",
                        DepartureTime = "12:00:00" },
                    new() { GtfsTripId = "T2", StopSequence = 3, GtfsStopId = "C", ArrivalTime = "14:00:00",
                        DepartureTime = "14:00:00" },
                    new() { GtfsTripId = "T2", StopSequence = 4, GtfsStopId = "D", ArrivalTime = "16:00:00",
                        DepartureTime = "16:00:00" },
                    new() { GtfsTripId = "T2", StopSequence = 5, GtfsStopId = "E", ArrivalTime = "23:59:59" }
                ]
            }
        },
        Services = new Dictionary<string, RealtimeServiceSnapshotModel>
        {
            ["S1"] = new()
            {
                GtfsServiceId = "S1", Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true,
                Saturday = true, Sunday = true, StartDate = DateOnly.MinValue, EndDate = DateOnly.MaxValue
            }
        }
    };

    private static FeedMessageModel Position(long timestamp, double latitude, double longitude) => new()
    {
        Header = new FeedHeaderModel { GtfsRealtimeVersion = "2.0", Timestamp = (ulong)timestamp },
        Entity =
        [
            new FeedEntityModel
            {
                Id = "V1",
                Vehicle = new VehiclePositionModel
                {
                    Vehicle = new VehicleDescriptorModel { Id = "V1" },
                    Trip = new TripDescriptorModel { TripId = "T1" },
                    Position = new PositionModel { Latitude = (float)latitude, Longitude = (float)longitude, Speed = 0 },
                    Timestamp = (ulong)timestamp
                }
            }
        ]
    };

    private static FeedMessageModel Updates(long timestamp, params TripUpdateModel[] updates) => new()
    {
        Header = new FeedHeaderModel { GtfsRealtimeVersion = "2.0", Timestamp = (ulong)timestamp },
        Entity = updates.Select(update => new FeedEntityModel { Id = update.Trip.TripId!, TripUpdate = update }).ToList()
    };

    private static TripUpdateModel Update(string tripId, params StopTimeUpdateModel[] stopTimeUpdates) => new()
    {
        Trip = new TripDescriptorModel { TripId = tripId }, StopTimeUpdate = stopTimeUpdates.ToList()
    };

    private static TripUpdateModel Delayed(string tripId, int delay)
        => Update(tripId, new StopTimeUpdateModel { StopSequence = 1, Departure = new StopTimeEventModel { Delay = delay } });

    private static FeedMessageModel Canceled(long timestamp) => new()
    {
        Header = new FeedHeaderModel { GtfsRealtimeVersion = "2.0", Timestamp = (ulong)timestamp },
        Entity =
        [
            new FeedEntityModel
            {
                Id = "T1",
                TripUpdate = new TripUpdateModel
                {
                    Trip = new TripDescriptorModel
                    {
                        TripId = "T1", ScheduleRelationship = TripScheduleRelationshipConsts.CanceledCode
                    }
                }
            }
        ]
    };
}
