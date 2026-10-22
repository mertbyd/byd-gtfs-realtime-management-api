using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;
using Byd.GtfsRealtimeManagement.Managers.Lookups;
using Byd.GtfsRealtimeManagement.Mappers.Lookups;

namespace Byd.GtfsRealtimeManagement.Services.Lookups;

// islevi: Realtime lookup satirlarinin olustur, oku, listele, guncelle ve sil akisini tek yerde yurutur.
// sistemdeki gorevi: Tipli repository, Manager ve Mapperly baglarini merkezde cozerek yaprak servisleri bos tutar.
public abstract class RealtimeLookupAppService<TEntity, TManager, TMapper>
    : GtfsRealtimeManagementAppService, IRealtimeLookupCrudAppService
    where TEntity : RealtimeLookup
    where TManager : BaseLookupManager<TEntity>
    where TMapper : IRealtimeLookupCrudMapper<TEntity>, new()
{
    private static readonly TMapper Mapper = new();
    private TManager Manager => LazyGetRequiredService<TManager>();
    private IRealtimeLookupRepository<TEntity> Repository => LazyGetRequiredService<IRealtimeLookupRepository<TEntity>>();

    // Girdiyi dogrular, yeni satiri kurar ve kalici hale getirir.
    public virtual async Task<RealtimeLookupDto> CreateAsync(
        CreateRealtimeLookupDto input, CancellationToken cancellationToken = default)
    {
        await LazyGetRequiredService<IValidator<CreateRealtimeLookupDto>>()
            .ValidateAndThrowAsync(input, cancellationToken);
        var entity = await Manager.CreateAsync(Mapper.Map(input), cancellationToken);
        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }

    // Kimligi verilen satiri getirir.
    public virtual async Task<RealtimeLookupDto> GetAsync(int id, CancellationToken cancellationToken = default)
        => Mapper.Map((await Manager.TryGetByIdAsync(id, cancellationToken: cancellationToken))!);

    // Turun tum satirlarini koda gore sirali getirir.
    public virtual async Task<List<RealtimeLookupDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetListAsync(cancellationToken: cancellationToken);
        return entities.OrderBy(value => value.Code).Select(value => Mapper.Map(value)).ToList();
    }

    // Girdiyi dogrular, satirin kodunu ve adini gunceller.
    public virtual async Task<RealtimeLookupDto> UpdateAsync(
        int id, UpdateRealtimeLookupDto input, CancellationToken cancellationToken = default)
    {
        await LazyGetRequiredService<IValidator<UpdateRealtimeLookupDto>>()
            .ValidateAndThrowAsync(input, cancellationToken);
        var entity = (await Manager.TryGetByIdAsync(id, cancellationToken: cancellationToken))!;
        await Manager.UpdateAsync(entity, Mapper.Map(input), cancellationToken);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }

    // Hicbir kayit kullanmiyorsa satiri kalici olarak siler.
    public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = (await Manager.TryGetByIdAsync(id, cancellationToken: cancellationToken))!;
        await Manager.EnsureCanDeleteAsync(entity, cancellationToken);
        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }
}
