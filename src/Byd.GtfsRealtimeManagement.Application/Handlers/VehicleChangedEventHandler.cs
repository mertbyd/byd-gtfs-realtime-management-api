using System.Threading.Tasks;
using Byd.GtfsRealtimeManagement.Etos.Vehicles;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;

namespace Byd.GtfsRealtimeManagement.Handlers;

// islevi: Arac modulunde eklenen, guncellenen ve silinen aracin kimligini realtime arac kopyasina uygular.
// sistemdeki gorevi: Arac kopyasi acilistan sonra veritabanina gitmeden guncel kalir; realtime'in kayit istegiyle eklenen
// arac da bu olayla kopyaya girer.
public class VehicleChangedEventHandler(RealtimeSnapshotManager snapshotManager)
    : IDistributedEventHandler<EntityCreatedEto<VehicleEto>>,
        IDistributedEventHandler<EntityUpdatedEto<VehicleEto>>,
        IDistributedEventHandler<EntityDeletedEto<VehicleEto>>,
        ITransientDependency
{
    public virtual Task HandleEventAsync(EntityCreatedEto<VehicleEto> eventData)
    {
        snapshotManager.SetVehicle(eventData.Entity);
        return Task.CompletedTask;
    }

    public virtual Task HandleEventAsync(EntityUpdatedEto<VehicleEto> eventData)
    {
        snapshotManager.SetVehicle(eventData.Entity);
        return Task.CompletedTask;
    }

    public virtual Task HandleEventAsync(EntityDeletedEto<VehicleEto> eventData)
    {
        snapshotManager.RemoveVehicle(eventData.Entity);
        return Task.CompletedTask;
    }
}
