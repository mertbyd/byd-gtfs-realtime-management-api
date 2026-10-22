using System.Threading;
using System.Threading.Tasks;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Managers.Lookups;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Byd.GtfsRealtimeManagement.Data.Lookups;

// islevi: Varsayilan realtime lookup satirlarini idempotent olarak eklemenin ortak akisini saglar.
// sistemdeki gorevi: Her lookup seeder'in ayni SeedAsync sablonunu yinelemesini onler.
public abstract class BaseLookupDataSeeder<TEntity, TManager>(
    TManager manager,
    IRepository<TEntity, int> repository) : IDataSeedContributor, ITransientDependency
    where TEntity : RealtimeLookup
    where TManager : BaseLookupManager<TEntity>
{
    // Eksik satirlari tek yazimda kaydeder; mevcut ad ve kodlara dokunmaz.
    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        var missing = await manager.CreateMissingAsync(CancellationToken.None);
        if (missing.Count == 0)
            return;
        await repository.InsertManyAsync(missing, autoSave: true, cancellationToken: CancellationToken.None);
    }
}
