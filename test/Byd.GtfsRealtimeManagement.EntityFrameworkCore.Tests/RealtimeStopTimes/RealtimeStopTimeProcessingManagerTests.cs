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

// islevi: Arac konumundan giris/cikis/gecis ve sefer guncellemesinden iptal durumunun durak satirlarina yazildigini dogrular.
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
        Assert.All(rows, row => Assert.Equal((canceledId, skippedId, "c-1"),
            (row.TripScheduleRelationshipId, row.StopScheduleRelationshipId, row.StatusCorrelationId)));
    }

    // Lookup'lari seed eder ve test organizasyonunda aktif kaynak kurar.
    private async Task<Guid> PrepareAsync()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await ServiceProvider.GetRequiredService<TripScheduleRelationshipDataSeeder>().SeedAsync(new DataSeedContext());
            await ServiceProvider.GetRequiredService<StopScheduleRelationshipDataSeeder>().SeedAsync(new DataSeedContext());
            await ServiceProvider.GetRequiredService<VehicleStopStatusDataSeeder>().SeedAsync(new DataSeedContext());
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

    private Task<List<RealtimeStopTime>> RowsAsync()
        => WithUnitOfWorkAsync(() => ServiceProvider.GetRequiredService<IRealtimeStopTimeRepository>().GetListAsync());

    // Duraklar 1 km arayla dizilidir; sefer UTC gunun tamaminda planlidir.
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
