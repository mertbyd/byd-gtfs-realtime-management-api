using System.Threading.Tasks;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Etos.RealtimeSources;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSources;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Byd.GtfsRealtimeManagement.Handlers;

// Organizasyonun aktif kaynagini ayni transaction icinde degistirir.
public class RealtimeSourceActivationRequestedEventHandler(
    RealtimeSourceManager sourceManager,
    ICurrentTenant currentTenant,
    ICurrentOrganization currentOrganization)
    : ILocalEventHandler<RealtimeSourceActivationRequestedEto>, ITransientDependency
{
    [UnitOfWork]
    public virtual async Task HandleEventAsync(RealtimeSourceActivationRequestedEto eventData)
    {
        using var tenantScope = currentTenant.Change(eventData.TenantId);
        using var organizationScope = currentOrganization.Change(eventData.OrganizationId);
        var source = await sourceManager.EnsureExistsIncludingInactiveInScopeAsync(eventData.RealtimeSourceId);
        await sourceManager.ActivateAsync(source);
    }
}
