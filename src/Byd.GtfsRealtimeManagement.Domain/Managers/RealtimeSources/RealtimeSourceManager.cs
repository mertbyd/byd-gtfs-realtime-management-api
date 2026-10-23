using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.ExceptionCodes;
using Byd.GtfsRealtimeManagement.Interfaces;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSources;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSources;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Settings;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace Byd.GtfsRealtimeManagement.Managers.RealtimeSources;

// islevi: Kaynak kapsamini, ad tekilligini, organizasyonun tek aktif kaynagini ve aktif kaynagin hangi akislarinin
// cekilecegini yonetir.
// sistemdeki gorevi: RealtimeSource is kurallarinin tek sahibidir.
public class RealtimeSourceManager(IRealtimeSourceRepository repository)
    : BaseManager<RealtimeSource, Guid>(repository)
{
    // Akis turlerinin kaynaktaki adres kolonu ve aralik ayari; yeni bir tur bu tabloya tek satirla eklenir.
    private static readonly (RealtimeFeedType FeedType, Func<RealtimeSource, string?> Url, string IntervalSettingName)[] Feeds =
    [
        (RealtimeFeedType.VehiclePositions, source => source.VehiclePositionsUrl,
            GtfsRealtimeManagementSettings.Fetch.VehiclePositionsIntervalSeconds),
        (RealtimeFeedType.TripUpdates, source => source.TripUpdatesUrl,
            GtfsRealtimeManagementSettings.Fetch.TripUpdatesIntervalSeconds),
        (RealtimeFeedType.ServiceAlerts, source => source.ServiceAlertsUrl,
            GtfsRealtimeManagementSettings.Fetch.ServiceAlertsIntervalSeconds)
    ];

    private ICurrentOrganization CurrentOrganization =>
        LazyServiceProvider.LazyGetRequiredService<ICurrentOrganization>();

    private IDataFilter DataFilter => LazyServiceProvider.LazyGetRequiredService<IDataFilter>();

    private IRealtimeFeedReader FeedReader => LazyServiceProvider.LazyGetRequiredService<IRealtimeFeedReader>();

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

    // Kaynagin adini ve adreslerini gunceller; etkinlik ayri islemle degisir.
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

    public virtual async Task<RealtimeSource> EnsureExistsInScopeAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        EnsureScopeAvailable();
        return (await TryGetByAsync(source => source.Id == id &&
            source.TenantId == CurrentTenant.Id && source.OrganizationId == CurrentOrganization.Id,
            cancellationToken: cancellationToken))!;
    }

    public virtual async Task<RealtimeSource> EnsureExistsIncludingInactiveInScopeAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        using var inactiveIncluded = DataFilter.Disable<ActiveRealtimeSourceFilter>();
        return await EnsureExistsInScopeAsync(id, cancellationToken);
    }

    // Organizasyonun aktif kaynagini getirir; akis okumalari istekten kaynak almadan bu kaynaktan yapilir.
    public virtual async Task<RealtimeSource> GetActiveInScopeAsync(CancellationToken cancellationToken = default)
    {
        EnsureScopeAvailable();
        return await Repository.FindAsync(source => source.TenantId == CurrentTenant.Id &&
            source.OrganizationId == CurrentOrganization.Id && source.IsActive,
            cancellationToken: cancellationToken)
            ?? throw new BusinessException(RealtimeSourceExceptionCodes.ActiveSourceRequired);
    }

    // Organizasyonun onceki aktif kaynagini pasife alip secilen kaynagi aktif yapar.
    public virtual async Task<RealtimeSource> ActivateAsync(
        RealtimeSource source, CancellationToken cancellationToken = default)
    {
        EnsureScopeAvailable();
        using var inactiveIncluded = DataFilter.Disable<ActiveRealtimeSourceFilter>();
        var active = await Repository.GetListAsync(candidate => candidate.TenantId == CurrentTenant.Id &&
            candidate.OrganizationId == CurrentOrganization.Id && candidate.IsActive && candidate.Id != source.Id,
            cancellationToken: cancellationToken);
        foreach (var previous in active)
        {
            previous.IsActive = false;
            await Repository.UpdateAsync(previous, autoSave: true, cancellationToken: cancellationToken);
        }

        source.IsActive = true;
        await Repository.UpdateAsync(source, autoSave: true, cancellationToken: cancellationToken);
        return source;
    }

    // Butun tenant ve organizasyonlarin aktif kaynaklarinin adresi dolu akislarini cekme hedefi olarak dondurur;
    // aktif kaynak filtresi acik kalir.
    public virtual async Task<List<RealtimeFeedTargetModel>> GetFeedTargetsAsync(CancellationToken cancellationToken = default)
    {
        using var allTenants = DataFilter.Disable<IMultiTenant>();
        using var allOrganizations = DataFilter.Disable<IMultiOrganization>();
        var sources = await Repository.GetListAsync(cancellationToken: cancellationToken);
        return sources.SelectMany(ToFeedTargets).ToList();
    }

    // Organizasyonun aktif kaynaginin VehiclePositions adresindeki arac konumlarini anlik okur.
    public virtual Task<List<VehiclePositionModel>> ReadVehiclePositionsAsync(
        CancellationToken cancellationToken = default)
        => ReadEntitiesAsync(RealtimeFeedType.VehiclePositions, entity => entity.Vehicle, cancellationToken);

    // Organizasyonun aktif kaynaginin TripUpdates adresindeki sefer guncellemelerini anlik okur.
    public virtual Task<List<TripUpdateModel>> ReadTripUpdatesAsync(CancellationToken cancellationToken = default)
        => ReadEntitiesAsync(RealtimeFeedType.TripUpdates, entity => entity.TripUpdate, cancellationToken);

    // Organizasyonun aktif kaynaginin ServiceAlerts adresindeki uyarilari anlik okur.
    public virtual Task<List<AlertModel>> ReadServiceAlertsAsync(CancellationToken cancellationToken = default)
        => ReadEntitiesAsync(RealtimeFeedType.ServiceAlerts, entity => entity.Alert, cancellationToken);

    // Aktif kaynagin adresi tanimli butun akislarini okur ve tek FeedMessage'da birlestirir.
    public virtual async Task<FeedMessageModel> ReadFeedMessageAsync(CancellationToken cancellationToken = default)
    {
        var source = await GetActiveInScopeAsync(cancellationToken);
        var messages = new List<FeedMessageModel>();
        foreach (var feed in Feeds.Where(feed => feed.Url(source) is not null))
            messages.Add(await FeedReader.ReadAsync(source.Name, feed.Url(source)!, cancellationToken));
        return Merge(messages);
    }

    // Aktif kaynagin tek akisini okur ve varliklarindan o akisin mesaj tipini secer; cagri ve ham yanit log modulune yazilir.
    private async Task<List<TMessage>> ReadEntitiesAsync<TMessage>(
        RealtimeFeedType feedType, Func<FeedEntityModel, TMessage?> select, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var source = await GetActiveInScopeAsync(cancellationToken);
        var message = await FeedReader.ReadAsync(source.Name, GetFeedUrl(source, feedType), cancellationToken);
        return message.Entity.Select(select).OfType<TMessage>().ToList();
    }

    // Birlesik baslik: en yeni standart surum ve en eski parcanin zamani; icerik ancak o an kadar gunceldir.
    private static FeedMessageModel Merge(List<FeedMessageModel> messages) => new()
    {
        Header = new FeedHeaderModel
        {
            GtfsRealtimeVersion = messages.Max(message => message.Header.GtfsRealtimeVersion)!,
            Incrementality = messages.Max(message => message.Header.Incrementality),
            Timestamp = messages.Min(message => message.Header.Timestamp),
            FeedVersion = messages.Select(message => message.Header.FeedVersion).FirstOrDefault(version => version is not null)
        },
        Entity = messages.SelectMany(message => message.Entity).ToList()
    };

    // Kaynagin istenen akisinin adresini dondurur; adres tanimli degilse kodlu hata verir.
    private static string GetFeedUrl(RealtimeSource source, RealtimeFeedType feedType)
        => Feeds.Single(feed => feed.FeedType == feedType).Url(source)
            ?? throw new BusinessException(RealtimeSourceExceptionCodes.FeedUrlNotConfigured);

    // Kaynagin adresi dolu her akisini kaynagin kapsami ve aralik ayariyla hedefe cevirir.
    private static IEnumerable<RealtimeFeedTargetModel> ToFeedTargets(RealtimeSource source) => Feeds
        .Where(feed => feed.Url(source) is not null)
        .Select(feed => new RealtimeFeedTargetModel
        {
            SourceId = source.Id,
            TenantId = source.TenantId,
            OrganizationId = source.OrganizationId,
            SourceName = source.Name,
            FeedType = feed.FeedType,
            Url = feed.Url(source)!,
            IntervalSettingName = feed.IntervalSettingName
        });

    // Alanlari normalize eder ve adin ayni organizasyonda tekil oldugunu dogrular.
    private async Task ApplyAsync(
        RealtimeSource entity, RealtimeSourceInputModel model, CancellationToken cancellationToken = default)
    {
        entity.Name = NormalizeRequiredText(model.Name);
        entity.VehiclePositionsUrl = NormalizeOptionalText(model.VehiclePositionsUrl);
        entity.TripUpdatesUrl = NormalizeOptionalText(model.TripUpdatesUrl);
        entity.ServiceAlertsUrl = NormalizeOptionalText(model.ServiceAlertsUrl);
        var name = entity.Name;
        using var inactiveIncluded = DataFilter.Disable<ActiveRealtimeSourceFilter>();
        await CheckAlreadyExistsAsync(value => value.OrganizationId == entity.OrganizationId &&
            value.TenantId == entity.TenantId && value.Name == name, entity.Id, cancellationToken);
    }

    private void EnsureScopeAvailable()
    {
        if (CurrentTenant.Id is null || CurrentOrganization.Id is null)
            throw new BusinessException(RealtimeSourceExceptionCodes.ScopeRequired);
    }
}
