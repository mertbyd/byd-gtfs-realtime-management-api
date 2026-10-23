using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Etos.Vehicles;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.RealtimeSnapshots;

// islevi: Arac ve plan kopyasinin yuklenmesini, kayitsiz arac isteklerini ve olaylarla guncellenmesini dogrular.
// sistemdeki gorevi: Dapper okumasi ve RabbitMQ yayini sahteyle degistirilerek kopya kurallari sinanir.
public class RealtimeSnapshotManagerTests : GtfsRealtimeManagementEntityFrameworkCoreTestBase
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private readonly IRealtimeSnapshotRepository _repository = Substitute.For<IRealtimeSnapshotRepository>();
    private readonly IDistributedEventBus _distributedEventBus = Substitute.For<IDistributedEventBus>();

    private RealtimeSnapshotManager Manager => ServiceProvider.GetRequiredService<RealtimeSnapshotManager>();

    // Dapper okumasini ve dagitik olay yolunu sahteyle degistirir; kopya bu sahte veriden kurulur.
    protected override void AfterAddApplication(IServiceCollection services)
    {
        _repository.GetVehiclesAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>())
            .Returns([new RealtimeVehicleSnapshotModel { VehicleId = Guid.NewGuid(), GtfsVehicleId = "1115" }]);
        _repository.GetActivePlanAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>())
            .Returns(new RealtimePlanSnapshotModel
            {
                FeedId = Guid.NewGuid(),
                Trips = new Dictionary<string, RealtimeTripSnapshotModel>
                {
                    ["T1"] = new()
                    {
                        GtfsTripId = "T1", GtfsServiceId = "S1", GtfsRouteId = "R1", RouteType = 3, GtfsAgencyId = "A1"
                    }
                },
                Services = new Dictionary<string, RealtimeServiceSnapshotModel>
                {
                    ["S1"] = new()
                    {
                        GtfsServiceId = "S1", Monday = true, Tuesday = true, Wednesday = true, Thursday = true,
                        Friday = true, StartDate = new DateOnly(2026, 10, 1), EndDate = new DateOnly(2026, 12, 31),
                        Exceptions = new Dictionary<DateOnly, int>
                        {
                            [new DateOnly(2026, 10, 28)] = RealtimeSnapshotConsts.ServiceRemovedExceptionType,
                            [new DateOnly(2026, 10, 25)] = RealtimeSnapshotConsts.ServiceAddedExceptionType
                        }
                    }
                }
            });
        services.Replace(ServiceDescriptor.Singleton(_repository));
        services.Replace(ServiceDescriptor.Singleton(_distributedEventBus));
    }

    [Fact]
    public async Task RequestUnregisteredVehicles_PublishesOncePerUnknownVehicleWithPlanRoute()
    {
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var message = Positions(("1115", "T1"), ("2000", "T1"), ("2000", "T1"), ("3000", null));

        await Manager.RequestUnregisteredVehiclesAsync(message);
        await Manager.RequestUnregisteredVehiclesAsync(message);

        await _distributedEventBus.Received(1).PublishAsync(
            Arg.Is<VehicleRegistrationRequestedEto>(request => request.GtfsVehicleId == "2000"
                && request.RouteType == 3 && request.GtfsAgencyId == "A1"
                && request.TenantId == TenantId && request.OrganizationId == OrganizationId),
            Arg.Any<bool>(), Arg.Any<bool>());
        await _distributedEventBus.Received(1).PublishAsync(
            Arg.Is<VehicleRegistrationRequestedEto>(request => request.GtfsVehicleId == "3000"
                && request.RouteType == null && request.GtfsAgencyId == null),
            Arg.Any<bool>(), Arg.Any<bool>());
        await _distributedEventBus.DidNotReceive().PublishAsync(
            Arg.Is<VehicleRegistrationRequestedEto>(request => request.GtfsVehicleId == "1115"),
            Arg.Any<bool>(), Arg.Any<bool>());
        await _repository.Received(1).GetVehiclesAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VehicleEvents_KeepCopyCurrent()
    {
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var vehicle = new VehicleEto
        {
            Id = Guid.NewGuid(), TenantId = TenantId, OrganizationId = OrganizationId, GtfsVehicleId = "2000"
        };
        var snapshot = await Manager.EnsureLoadedAsync();

        Manager.SetVehicle(vehicle);
        Assert.Equal(vehicle.Id, snapshot.VehicleIds["2000"]);
        vehicle.GtfsVehicleId = "2001";
        Manager.SetVehicle(vehicle);
        Assert.False(snapshot.VehicleIds.ContainsKey("2000"));
        Assert.Equal(vehicle.Id, snapshot.VehicleIds["2001"]);

        Manager.RemoveVehicle(vehicle);
        await Manager.RequestUnregisteredVehiclesAsync(Positions(("2001", null)));
        await _distributedEventBus.Received(1).PublishAsync(
            Arg.Is<VehicleRegistrationRequestedEto>(request => request.GtfsVehicleId == "2001"),
            Arg.Any<bool>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task ReloadPlan_OnlyRefreshesLoadedOrganization()
    {
        await Manager.ReloadPlanAsync(TenantId, OrganizationId);
        await _repository.DidNotReceive().GetActivePlanAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>());

        using (ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId))
        using (ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId))
            await Manager.EnsureLoadedAsync();
        await Manager.ReloadPlanAsync(TenantId, OrganizationId);

        await _repository.Received(2).GetActivePlanAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsTripScheduled_FollowsWeeklyCalendarAndDateExceptions()
    {
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var plan = (await Manager.EnsureLoadedAsync()).Plan!;

        Assert.True(Manager.IsTripScheduled(plan, "T1", new DateOnly(2026, 10, 23)));
        Assert.False(Manager.IsTripScheduled(plan, "T1", new DateOnly(2026, 10, 24)));
        Assert.True(Manager.IsTripScheduled(plan, "T1", new DateOnly(2026, 10, 25)));
        Assert.False(Manager.IsTripScheduled(plan, "T1", new DateOnly(2026, 10, 28)));
        Assert.False(Manager.IsTripScheduled(plan, "T1", new DateOnly(2027, 1, 4)));
        Assert.False(Manager.IsTripScheduled(plan, "UNKNOWN", new DateOnly(2026, 10, 23)));
    }

    // Verilen arac ve sefer kimlikleriyle arac konumu akisi kurar.
    private static FeedMessageModel Positions(params (string VehicleId, string? TripId)[] positions)
    {
        var message = new FeedMessageModel { Header = new FeedHeaderModel { GtfsRealtimeVersion = "2.0" } };
        foreach (var (vehicleId, tripId) in positions)
            message.Entity.Add(new FeedEntityModel
            {
                Id = vehicleId,
                Vehicle = new VehiclePositionModel
                {
                    Vehicle = new VehicleDescriptorModel { Id = vehicleId, Label = vehicleId },
                    Trip = tripId is null ? null : new TripDescriptorModel { TripId = tripId }
                }
            });
        return message;
    }
}
