using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.ExceptionCodes;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;
using Byd.GtfsRealtimeManagement.Models.Lookups;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace Byd.GtfsRealtimeManagement.Managers.Lookups;

// islevi: Lookup kodunun tekilligini, adin normalizasyonunu, eksik varsayilan satirlarin belirlenmesini ve kullanilan
// satirin silinmemesini yonetir.
// sistemdeki gorevi: Bes lookup turunun ortak yazma, seed ve silme kurallarini tek yerde uygular.
public abstract class BaseLookupManager<TEntity>(IRealtimeLookupRepository<TEntity> repository)
    : BaseManager<TEntity, int>(repository)
    where TEntity : RealtimeLookup
{
    private IDataFilter DataFilter => LazyServiceProvider.LazyGetRequiredService<IDataFilter>();

    // Alt sinifin Domain.Shared icindeki varsayilan satirlarini dondurur.
    protected abstract IReadOnlyList<(int Id, string Name, int Code)> GetDefinitions();

    // Tanimli kodu tipli entity olarak kurar.
    protected abstract TEntity CreateEntity(int id, string name, int code);

    // Kodun turde bos oldugunu dogrular ve yeni satiri kurar; kimligi veritabani uretir.
    public virtual async Task<TEntity> CreateAsync(
        RealtimeLookupCreateModel model, CancellationToken cancellationToken = default)
    {
        await EnsureCodeAvailableAsync(model.Code, 0, cancellationToken);
        return CreateEntity(0, model.Name.Trim(), model.Code);
    }

    // Kodun baska bir satirda olmadigini dogrular ve kod ile adi gunceller.
    public virtual async Task UpdateAsync(
        TEntity entity, RealtimeLookupUpdateModel model, CancellationToken cancellationToken = default)
    {
        await EnsureCodeAvailableAsync(model.Code, entity.Id, cancellationToken);
        entity.Code = model.Code;
        entity.Name = model.Name.Trim();
    }

    // Kimligi ya da kodu henuz kullanilmayan varsayilan satirlari kurar; mevcut ve kullanicinin
    // degistirdigi satirlara dokunmaz.
    public virtual async Task<List<TEntity>> CreateMissingAsync(CancellationToken cancellationToken = default)
    {
        var existing = await Repository.GetListAsync(cancellationToken: cancellationToken);
        var ids = existing.Select(value => value.Id).ToHashSet();
        var codes = existing.Select(value => value.Code).ToHashSet();
        return GetDefinitions().Where(value => !ids.Contains(value.Id) && !codes.Contains(value.Code))
            .Select(value => CreateEntity(value.Id, value.Name, value.Code)).ToList();
    }

    // Satiri herhangi bir tenant veya organizasyondaki kayit kullaniyorsa silmeyi engeller. Lookup host kapsaminda
    // oldugu icin kontrol boyunca tenant ve organizasyon filtreleri kapatilir.
    public virtual async Task EnsureCanDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using (DataFilter.Disable<IMultiTenant>())
        using (DataFilter.Disable<IMultiOrganization>())
        {
            if (await Repository.IsDataUsedAsync(entity, ignoredEntityTypes: null, cancellationToken))
                throw new BusinessException(GeneralExceptionCodes.RelationalDataDelete);
        }
    }

    // Ayni kodun turde baska bir satirda bulunmasini kodlu hatayla engeller.
    private async Task EnsureCodeAvailableAsync(int code, int currentId, CancellationToken cancellationToken = default)
    {
        if (await Repository.FindAsync(
                value => value.Code == code && value.Id != currentId, cancellationToken: cancellationToken) is not null)
            throw new BusinessException(RealtimeLookupExceptionCodes.CodeAlreadyExists);
    }
}
