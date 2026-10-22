using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Byd.GtfsRealtimeManagement.ExceptionCodes;
using Byd.GtfsRealtimeManagement.Services.RealtimeSources;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.RealtimeSources;

public class RealtimeSourceCrudTests : GtfsRealtimeManagementEntityFrameworkCoreTestBase
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid OtherOrganizationId = Guid.NewGuid();

    private ICurrentTenant CurrentTenant => ServiceProvider.GetRequiredService<ICurrentTenant>();
    private ICurrentOrganization CurrentOrganization => ServiceProvider.GetRequiredService<ICurrentOrganization>();
    private IRealtimeSourceAppService SourceService => ServiceProvider.GetRequiredService<IRealtimeSourceAppService>();

    [Fact]
    public async Task SourceService_CreatesReadsUpdatesAndDeletes_InOrganization()
    {
        using var tenant = CurrentTenant.Change(TenantId);
        using var organization = CurrentOrganization.Change(OrganizationId);
        var created = await WithUnitOfWorkAsync(() => SourceService.CreateAsync(NewSource("Vilnius")));
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("https://example.org/vehicles.pb", created.VehiclePositionsUrl);
        Assert.Equal(created.Name, (await WithUnitOfWorkAsync(() => SourceService.GetAsync(created.Id))).Name);

        var duplicate = await Assert.ThrowsAsync<BusinessException>(() =>
            WithUnitOfWorkAsync(() => SourceService.CreateAsync(NewSource("Vilnius"))));
        Assert.Equal(GeneralExceptionCodes.AlreadyExists, duplicate.Code);

        var updated = await WithUnitOfWorkAsync(() => SourceService.UpdateAsync(created.Id,
            new UpdateRealtimeSourceDto
            {
                Name = "Vilnius Updated", TripUpdatesUrl = "https://example.org/trips.pb", IsActive = false
            }));
        Assert.False(updated.IsActive);
        Assert.Null(updated.VehiclePositionsUrl);
        Assert.Equal("https://example.org/trips.pb", updated.TripUpdatesUrl);
        var page = await WithUnitOfWorkAsync(() =>
            SourceService.GetFilteredListAsync(new DataTableListInput<RealtimeSourceFilter>()));
        Assert.Equal(1, page.TotalCount);
        Assert.False(page.Items[0].IsActive);
        var filtered = await WithUnitOfWorkAsync(() => SourceService.GetFilteredListAsync(
            new DataTableListInput<RealtimeSourceFilter>
            {
                Filters = new RealtimeSourceFilter
                {
                    Name = new DataTableFilterItem
                    {
                        Operator = DataTableFilterOperator.Equal, Value = "Vilnius Updated"
                    }
                }
            }));
        Assert.Equal(1, filtered.TotalCount);

        await WithUnitOfWorkAsync(() => SourceService.DeleteAsync(created.Id));
        await Assert.ThrowsAsync<BusinessException>(() =>
            WithUnitOfWorkAsync(() => SourceService.GetAsync(created.Id)));
    }

    [Fact]
    public async Task SourceService_IsolatesOrganizations_AndRequiresScope()
    {
        using var tenant = CurrentTenant.Change(TenantId);
        var withoutOrganization = await Assert.ThrowsAsync<BusinessException>(() =>
            WithUnitOfWorkAsync(() => SourceService.CreateAsync(NewSource("Vilnius"))));
        Assert.Equal(RealtimeSourceExceptionCodes.ScopeRequired, withoutOrganization.Code);

        Guid firstId;
        using (CurrentOrganization.Change(OrganizationId))
        {
            firstId = (await WithUnitOfWorkAsync(() => SourceService.CreateAsync(NewSource("Vilnius")))).Id;
        }

        using (CurrentOrganization.Change(OtherOrganizationId))
        {
            await WithUnitOfWorkAsync(() => SourceService.CreateAsync(NewSource("Vilnius")));
            var page = await WithUnitOfWorkAsync(() =>
                SourceService.GetFilteredListAsync(new DataTableListInput<RealtimeSourceFilter>()));
            Assert.Equal(1, page.TotalCount);
            await Assert.ThrowsAsync<BusinessException>(() =>
                WithUnitOfWorkAsync(() => SourceService.GetAsync(firstId)));
            await Assert.ThrowsAsync<BusinessException>(() =>
                WithUnitOfWorkAsync(() => SourceService.DeleteAsync(firstId)));
        }
    }

    [Fact]
    public async Task SourceService_RequiresAtLeastOneHttpUrl()
    {
        using var tenant = CurrentTenant.Change(TenantId);
        using var organization = CurrentOrganization.Change(OrganizationId);
        var noUrl = NewSource("Empty");
        noUrl.VehiclePositionsUrl = null;
        await Assert.ThrowsAsync<AbpValidationException>(() =>
            WithUnitOfWorkAsync(() => SourceService.CreateAsync(noUrl)));

        var invalid = NewSource("Invalid");
        invalid.VehiclePositionsUrl = "file:///private/feed.pb";
        await Assert.ThrowsAsync<AbpValidationException>(() =>
            WithUnitOfWorkAsync(() => SourceService.CreateAsync(invalid)));
    }

    private static CreateRealtimeSourceDto NewSource(string name) => new()
    {
        Name = name,
        VehiclePositionsUrl = "https://example.org/vehicles.pb",
        IsActive = true
    };
}
