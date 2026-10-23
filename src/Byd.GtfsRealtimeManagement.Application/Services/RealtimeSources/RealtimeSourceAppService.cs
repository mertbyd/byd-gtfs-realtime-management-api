using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSources;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSources;
using Byd.GtfsRealtimeManagement.Mappers.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Mappers.RealtimeSources;
using Byd.GtfsRealtimeManagement.Interfaces;
using Byd.GtfsRealtimeManagement.Etos.RealtimeSources;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Application.Dtos;

namespace Byd.GtfsRealtimeManagement.Services.RealtimeSources;

// islevi: Kaynak olustur, oku, filtreli listele, guncelle, etkinlestir ve sil akisini; aktif kaynagin akislarini anlik
// okumayi yurutur.
// sistemdeki gorevi: Controller ile Manager/Repository zinciri arasindaki public giristir.
public class RealtimeSourceAppService : GtfsRealtimeManagementAppService, IRealtimeSourceAppService
{
    private static readonly RealtimeSourceMapper Mapper = new();
    private static readonly RealtimeFeedMapper FeedMapper = new();
    private RealtimeSourceManager Manager => LazyGetRequiredService<RealtimeSourceManager>();
    private IRealtimeSourceRepository Repository => LazyGetRequiredService<IRealtimeSourceRepository>();
    private IDataFilter ActiveFilter => LazyGetRequiredService<IDataFilter>();
    private ILocalEventBus LocalEventBus => LazyGetRequiredService<ILocalEventBus>();
    private IValidator<TInput> Validator<TInput>() => LazyGetRequiredService<IValidator<TInput>>();

    // Girdiyi dogrular, kapsamdaki kaynagi kurar ve kaydeder.
    public virtual async Task<RealtimeSourceDto> CreateAsync(
        CreateRealtimeSourceDto input, CancellationToken cancellationToken = default)
    {
        await Validator<CreateRealtimeSourceDto>().ValidateAndThrowAsync(input, cancellationToken);
        var entity = await Manager.CreateAsync(Mapper.Map(input), cancellationToken);
        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }

    // Aktif kaynagi id ile istegin kapsaminda getirir.
    public virtual async Task<RealtimeSourceDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Mapper.Map(await Manager.EnsureExistsInScopeAsync(id, cancellationToken));

    // Yonetim icin pasif kaynagi da istegin kapsaminda getirir.
    public virtual async Task<RealtimeSourceDto> GetIncludingInactiveAsync(
        Guid id, CancellationToken cancellationToken = default)
        => Mapper.Map(await Manager.EnsureExistsIncludingInactiveInScopeAsync(id, cancellationToken));

    // Pasifler dahil kaynaklarin filtreli sayfasini ve toplam kayit sayisini getirir.
    public virtual async Task<PagedResultDto<RealtimeSourceDto>> GetFilteredListAsync(
        DataTableListInput<RealtimeSourceFilter> input, CancellationToken cancellationToken = default)
    {
        await Validator<DataTableListInput<RealtimeSourceFilter>>().ValidateAndThrowAsync(input, cancellationToken);
        using var inactiveIncluded = ActiveFilter.Disable<ActiveRealtimeSourceFilter>();
        var entities = await Repository.GetFilteredListAsync(input, cancellationToken);
        var totalCount = await Repository.GetFilteredCountAsync(input, cancellationToken);
        return new PagedResultDto<RealtimeSourceDto>(totalCount, entities.Select(Mapper.Map).ToList());
    }

    // Girdiyi dogrular, kapsamdaki kaynagi Manager ile gunceller ve kaydeder.
    public virtual async Task<RealtimeSourceDto> UpdateAsync(
        Guid id, UpdateRealtimeSourceDto input, CancellationToken cancellationToken = default)
    {
        await Validator<UpdateRealtimeSourceDto>().ValidateAndThrowAsync(input, cancellationToken);
        var entity = await Manager.EnsureExistsIncludingInactiveInScopeAsync(id, cancellationToken);
        await Manager.UpdateAsync(entity, Mapper.Map(input), cancellationToken);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }

    // Kaynagi bagli kayit yoksa siler.
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Manager.EnsureExistsIncludingInactiveInScopeAsync(id, cancellationToken);
        await Manager.EnsureCanDeleteAsync(entity, cancellationToken);
        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }

    // Yerel handler ayni transaction'da organizasyonun aktif kaynagini degistirir.
    public virtual async Task<RealtimeSourceDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await Manager.EnsureExistsIncludingInactiveInScopeAsync(id, cancellationToken);
        await LocalEventBus.PublishAsync(new RealtimeSourceActivationRequestedEto
        {
            TenantId = source.TenantId!.Value,
            OrganizationId = source.OrganizationId!.Value,
            RealtimeSourceId = source.Id
        }, onUnitOfWorkComplete: false);
        return Mapper.Map(source);
    }

    // Organizasyonun aktif kaynaginin arac konumlarini anlik ceker.
    public virtual async Task<List<VehiclePositionDto>> GetVehiclePositionsAsync(
        CancellationToken cancellationToken = default)
        => FeedMapper.Map(await Manager.ReadVehiclePositionsAsync(cancellationToken));

    // Organizasyonun aktif kaynaginin sefer guncellemelerini anlik ceker.
    public virtual async Task<List<TripUpdateDto>> GetTripUpdatesAsync(CancellationToken cancellationToken = default)
        => FeedMapper.Map(await Manager.ReadTripUpdatesAsync(cancellationToken));

    // Organizasyonun aktif kaynaginin uyarilarini anlik ceker.
    public virtual async Task<List<AlertDto>> GetServiceAlertsAsync(CancellationToken cancellationToken = default)
        => FeedMapper.Map(await Manager.ReadServiceAlertsAsync(cancellationToken));

    // Organizasyonun aktif kaynaginin butun akislarini tek FeedMessage olarak anlik ceker.
    public virtual async Task<FeedMessageDto> GetFeedMessageAsync(CancellationToken cancellationToken = default)
        => FeedMapper.Map(await Manager.ReadFeedMessageAsync(cancellationToken));
}
