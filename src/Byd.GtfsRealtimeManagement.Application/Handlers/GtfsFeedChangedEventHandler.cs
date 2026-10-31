using System.Threading.Tasks;
using Byd.GtfsRealtimeManagement.Etos.GtfsFeeds;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Byd.GtfsRealtimeManagement.Handlers;

// islevi: GTFS modulunde organizasyonun aktif feed'i ya da aktif feed'in icerigi degisince plan kopyasini yeniden yukler.
// sistemdeki gorevi: Plan kopyasi periyodik sorgu atmadan, degisiklik aninda guncel kalir.
public class GtfsFeedChangedEventHandler(RealtimeSnapshotManager snapshotManager)
    : IDistributedEventHandler<GtfsFeedChangedEto>, ITransientDependency
{
    public virtual Task HandleEventAsync(GtfsFeedChangedEto eventData)
        => snapshotManager.ReloadPlanAsync(eventData.TenantId, eventData.OrganizationId);
}
