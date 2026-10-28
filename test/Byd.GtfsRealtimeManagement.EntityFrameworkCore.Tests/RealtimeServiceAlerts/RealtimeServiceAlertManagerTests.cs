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
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Managers.RealtimeServiceAlerts;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.RealtimeServiceAlerts;

// islevi: Servis uyarilarindan yalniz gosterim araliginda olan ve kapsami planda karsiligi bulunanlarin tam liste olarak
// dondugunu dogrular.
// sistemdeki gorevi: Plan kopyasi sahteyle verilir; uyarilar standarttaki varsayilan kodlarla gosterilir.
public class RealtimeServiceAlertManagerTests : GtfsRealtimeManagementEntityFrameworkCoreTestBase
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private readonly IRealtimeSnapshotRepository _snapshotRepository = Substitute.For<IRealtimeSnapshotRepository>();

    private RealtimeServiceAlertManager Alerts => ServiceProvider.GetRequiredService<RealtimeServiceAlertManager>();

    // Planda R1 hatti, A duragi ve T1 seferi vardir.
    protected override void AfterAddApplication(IServiceCollection services)
    {
        _snapshotRepository.GetVehiclesAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>()).Returns([]);
        _snapshotRepository.GetActivePlanAsync(TenantId, OrganizationId, Arg.Any<CancellationToken>()).Returns(
            new RealtimePlanSnapshotModel
            {
                FeedId = Guid.NewGuid(), RouteIds = ["R1"], StopIds = ["A"],
                Trips = new Dictionary<string, RealtimeTripSnapshotModel>
                {
                    ["T1"] = new() { GtfsTripId = "T1", GtfsServiceId = "S1", GtfsRouteId = "R1" }
                }
            });
        services.Replace(ServiceDescriptor.Singleton(_snapshotRepository));
    }

    [Fact]
    public async Task ProcessAsync_ShowsOnlyActiveAlertsMatchingThePlan()
    {
        using var tenant = ServiceProvider.GetRequiredService<ICurrentTenant>().Change(TenantId);
        using var organization = ServiceProvider.GetRequiredService<ICurrentOrganization>().Change(OrganizationId);
        var now = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var stream = await Alerts.ProcessAsync(new FeedMessageModel
        {
            Header = new FeedHeaderModel { GtfsRealtimeVersion = "2.0", Timestamp = now },
            Entity =
            [
                Alert("route-now", new EntitySelectorModel { RouteId = "R1" }, new TimeRangeModel { Start = now - 60, End = now + 60 }),
                Alert("unknown-route", new EntitySelectorModel { RouteId = "X" }),
                Alert("expired", new EntitySelectorModel { RouteId = "R1" }, new TimeRangeModel { End = now - 60 }),
                Alert("stop-always", new EntitySelectorModel { StopId = "A" }),
                Alert("direction-only", new EntitySelectorModel { DirectionId = 1 })
            ]
        });

        Assert.True(stream.IsFullDataset);
        Assert.Equal(["route-now", "stop-always"], stream.Entities.Select(alert => alert.AlertId));
        Assert.All(stream.Entities, alert => Assert.Equal(ServiceAlertConsts.UnknownEffectCode, alert.Effect));
    }

    private static FeedEntityModel Alert(string id, EntitySelectorModel selector, params TimeRangeModel[] periods) => new()
    {
        Id = id,
        Alert = new AlertModel { InformedEntity = [selector], CommunicationPeriod = periods.ToList() }
    };
}
