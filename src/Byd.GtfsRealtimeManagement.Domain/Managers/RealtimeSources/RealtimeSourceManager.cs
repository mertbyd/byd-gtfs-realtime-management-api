using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.ExceptionCodes;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSources;
using Byd.GtfsRealtimeManagement.Models.RealtimeSources;
using Volo.Abp;

namespace Byd.GtfsRealtimeManagement.Managers.RealtimeSources;

// islevi: Kaynak kapsamini, ad tekilligini ve duzenlenebilir alanlari yonetir.
// sistemdeki gorevi: RealtimeSource is kurallarinin tek sahibidir.
public class RealtimeSourceManager(IRealtimeSourceRepository repository)
    : BaseManager<RealtimeSource, Guid>(repository)
{
    private ICurrentOrganization CurrentOrganization =>
        LazyServiceProvider.LazyGetRequiredService<ICurrentOrganization>();

    // Istegin tenant ve organizasyonunda yeni kaynak kurar.
    public virtual async Task<RealtimeSource> CreateAsync(
        RealtimeSourceCreateModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (CurrentTenant.Id is not { } tenantId || CurrentOrganization.Id is not { } organizationId)
            throw new BusinessException(RealtimeSourceExceptionCodes.ScopeRequired);
        var entity = new RealtimeSource(GuidGenerator.Create(), tenantId, organizationId);
        await ApplyAsync(entity, model, cancellationToken);
        return entity;
    }

    // Kaynagin adini, adreslerini ve etkinligini gunceller.
    public virtual Task UpdateAsync(
        RealtimeSource entity, RealtimeSourceUpdateModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(model);
        return ApplyAsync(entity, model, cancellationToken);
    }

    // Bagli kayit varsa kaynagin silinmesini engeller.
    public virtual async Task EnsureCanDeleteAsync(
        RealtimeSource entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (await Repository.IsDataUsedAsync(entity, ignoredEntityTypes: null, cancellationToken))
            throw new BusinessException(GeneralExceptionCodes.RelationalDataDelete);
    }

    // Alanlari normalize eder ve adin ayni organizasyonda tekil oldugunu dogrular.
    private async Task ApplyAsync(
        RealtimeSource entity, RealtimeSourceInputModel model, CancellationToken cancellationToken = default)
    {
        entity.Name = NormalizeRequiredText(model.Name);
        entity.VehiclePositionsUrl = NormalizeOptionalText(model.VehiclePositionsUrl);
        entity.TripUpdatesUrl = NormalizeOptionalText(model.TripUpdatesUrl);
        entity.ServiceAlertsUrl = NormalizeOptionalText(model.ServiceAlertsUrl);
        entity.IsActive = model.IsActive;
        var name = entity.Name;
        await CheckAlreadyExistsAsync(value => value.Name == name, entity.Id, cancellationToken);
    }
}
