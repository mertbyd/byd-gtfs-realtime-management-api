using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Data.Lookups;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Services.RealtimeSources;
using Byd.GtfsRealtimeManagement.Services.RealtimeStopTimes;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.RealtimeStopTimes;

// islevi: Durak olcumu CRUD ve kaynak degisimindeki gorunurlugu dogrular.
// sistemdeki gorevi: Kapsam, tekillik ve lookup adlari icin entegrasyon guvencesidir.
public class RealtimeStopTimeCrudTests : GtfsRealtimeManagementEntityFrameworkCoreTestBase
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid OtherOrganizationId = Guid.NewGuid();

    private ICurrentTenant CurrentTenant => ServiceProvider.GetRequiredService<ICurrentTenant>();
    private ICurrentOrganization CurrentOrganization => ServiceProvider.GetRequiredService<ICurrentOrganization>();
    private IRealtimeSourceAppService Sources => ServiceProvider.GetRequiredService<IRealtimeSourceAppService>();
    private IRealtimeStopTimeAppService StopTimes => ServiceProvider.GetRequiredService<IRealtimeStopTimeAppService>();

    [Fact]
    public async Task Crud_UsesActiveSourceAndPreventsDuplicateTripStop()
    {
        await SeedRelationshipsAsync();
        using var tenant = CurrentTenant.Change(TenantId);
        using var organization = CurrentOrganization.Change(OrganizationId);
        var source = await CreateActiveSourceAsync("First");

        var created = await WithUnitOfWorkAsync(() => StopTimes.CreateAsync(NewStopTime()));
        Assert.Equal(source.Id, created.RealtimeSourceId);
        Assert.Equal("First", created.RealtimeSourceName);
        Assert.Equal("Planlı", created.TripScheduleRelationshipName);
        Assert.Equal("Planlı", created.StopScheduleRelationshipName);
        Assert.Equal("Planlı", (await WithUnitOfWorkAsync(() => StopTimes.GetAsync(created.Id)))
            .TripScheduleRelationshipName);

        var duplicate = await Assert.ThrowsAsync<BusinessException>(() =>
            WithUnitOfWorkAsync(() => StopTimes.CreateAsync(NewStopTime())));
        Assert.Equal(GeneralExceptionCodes.AlreadyExists, duplicate.Code);

        var update = NewStopTimeUpdate();
        update.ReportedDelay = 120;
        var updated = await WithUnitOfWorkAsync(() => StopTimes.UpdateAsync(created.Id, update));
        Assert.Equal(120, updated.ReportedDelay);
        var page = await WithUnitOfWorkAsync(() => StopTimes.GetFilteredListAsync(
            new DataTableListInput<RealtimeStopTimeFilter>
            {
                Filters = new RealtimeStopTimeFilter
                {
                    GtfsRouteId = new DataTableFilterItem
                    {
                        Operator = DataTableFilterOperator.Equal, Value = "route-1"
                    }
                }
            }));
        Assert.Equal(1, page.TotalCount);
        Assert.Equal("First", page.Items[0].RealtimeSourceName);

        await WithUnitOfWorkAsync(() => StopTimes.DeleteAsync(created.Id));
        await Assert.ThrowsAsync<BusinessException>(() =>
            WithUnitOfWorkAsync(() => StopTimes.GetAsync(created.Id)));
    }

    [Fact]
    public async Task SourceSwitch_HidesOldRows_WhileHistoryRetainsScope()
    {
        await SeedRelationshipsAsync();
        using var tenant = CurrentTenant.Change(TenantId);
        Guid oldStopId;
        using (CurrentOrganization.Change(OrganizationId))
        {
            var oldSource = await CreateActiveSourceAsync("First");
            oldStopId = (await WithUnitOfWorkAsync(() => StopTimes.CreateAsync(NewStopTime()))).Id;
            await CreateActiveSourceAsync("Second");

            Assert.Equal(0, (await WithUnitOfWorkAsync(() => StopTimes.GetFilteredListAsync(
                new DataTableListInput<RealtimeStopTimeFilter>()))).TotalCount);
            Assert.Equal(1, (await WithUnitOfWorkAsync(() => StopTimes.GetFilteredListIncludingInactiveAsync(
                new DataTableListInput<RealtimeStopTimeFilter>()))).TotalCount);
            await Assert.ThrowsAsync<BusinessException>(() =>
                WithUnitOfWorkAsync(() => StopTimes.GetAsync(oldStopId)));
            Assert.Equal("First", (await WithUnitOfWorkAsync(() =>
                StopTimes.GetIncludingInactiveAsync(oldStopId))).RealtimeSourceName);
            var deleteError = await Assert.ThrowsAsync<BusinessException>(() =>
                WithUnitOfWorkAsync(() => Sources.DeleteAsync(oldSource.Id)));
            Assert.Equal(GeneralExceptionCodes.RelationalDataDelete, deleteError.Code);
        }

        using (CurrentOrganization.Change(OtherOrganizationId))
        {
            Assert.Equal(0, (await WithUnitOfWorkAsync(() => StopTimes.GetFilteredListIncludingInactiveAsync(
                new DataTableListInput<RealtimeStopTimeFilter>()))).TotalCount);
            await Assert.ThrowsAsync<BusinessException>(() =>
                WithUnitOfWorkAsync(() => StopTimes.GetIncludingInactiveAsync(oldStopId)));
        }

        using (CurrentTenant.Change(Guid.NewGuid()))
        using (CurrentOrganization.Change(OrganizationId))
        {
            Assert.Equal(0, (await WithUnitOfWorkAsync(() => StopTimes.GetFilteredListIncludingInactiveAsync(
                new DataTableListInput<RealtimeStopTimeFilter>()))).TotalCount);
            await Assert.ThrowsAsync<BusinessException>(() =>
                WithUnitOfWorkAsync(() => StopTimes.GetIncludingInactiveAsync(oldStopId)));
        }
    }

    // Seed verilerini host kapsamindaki lookup tablolara ekler.
    private Task SeedRelationshipsAsync() => WithUnitOfWorkAsync(async () =>
    {
        await ServiceProvider.GetRequiredService<TripScheduleRelationshipDataSeeder>()
            .SeedAsync(new DataSeedContext());
        await ServiceProvider.GetRequiredService<StopScheduleRelationshipDataSeeder>()
            .SeedAsync(new DataSeedContext());
    });

    // Test organizasyonunda pasif kaynagi olusturup etkinlestirir.
    private async Task<RealtimeSourceDto> CreateActiveSourceAsync(string name)
    {
        var source = await WithUnitOfWorkAsync(() => Sources.CreateAsync(new CreateRealtimeSourceDto
        {
            Name = name, VehiclePositionsUrl = "https://example.org/positions.pb"
        }));
        return await WithUnitOfWorkAsync(() => Sources.ActivateAsync(source.Id));
    }

    // Ayni gun ve seferde tek durak icin gecerli istek kurar.
    private static CreateRealtimeStopTimeDto NewStopTime() => new()
    {
        TripScheduleRelationshipId = 1,
        StopScheduleRelationshipId = 1,
        GtfsFeedId = Guid.NewGuid(),
        ServiceDate = new DateOnly(2026, 10, 23),
        GtfsTripId = "trip-1",
        GtfsRouteId = "route-1",
        StopSequence = 1,
        GtfsStopId = "stop-1"
    };

    // Update istegini create ile ayni is kimliginde kurar.
    private static UpdateRealtimeStopTimeDto NewStopTimeUpdate() => new()
    {
        TripScheduleRelationshipId = 1,
        StopScheduleRelationshipId = 1,
        GtfsFeedId = Guid.NewGuid(),
        ServiceDate = new DateOnly(2026, 10, 23),
        GtfsTripId = "trip-1",
        GtfsRouteId = "route-1",
        StopSequence = 1,
        GtfsStopId = "stop-1"
    };
}
