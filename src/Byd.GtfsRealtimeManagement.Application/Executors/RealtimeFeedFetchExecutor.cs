using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Managers.RealtimeServiceAlerts;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeStreams;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;
using Byd.LogManagement.Interfaces.Notifications;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;

namespace Byd.GtfsRealtimeManagement.Executors;

/// <summary>
/// Bir kaynagin tek akisini kaynagin kapsaminda ve araligi dolduysa ceker.
/// </summary>
// islevi: Hedefin tenant ve organizasyon kapsamina girer; araligi dolmus akisi isaretleyip okuyucuya ceker.
// sistemdeki gorevi: Cekme iscisinin her hedef icin calistirdigi adimdir. Aralik organizasyon ayarindan, cagrinin kaydi ve kapsami
// log modulunde bu kapsamdan yazilir. Her cekme kendi correlation kimligiyle yapilir; ham kayit ve islenmis durak satiri
// ayni kimligi tasir. Her akis kendi isleme adimina verilir; araclarin son durumu, seferlerin tahmini durak zamanlari ve
// gecerli servis uyarilari log modulunun canli akisiyla, akis turunun adini tasiyarak organizasyona itilir. Canli akis kalici
// degildir: olay kutusuna ve bildirim tablosuna yazilmaz, kacan mesajin yerini bir sonraki alir; istemci istek atmaz.
public class RealtimeFeedFetchExecutor(
    IRealtimeFeedReader reader,
    ISettingProvider settingProvider,
    IDistributedCache<RealtimeFeedFetchCacheItem> fetchCache,
    IClock clock,
    ICurrentTenant currentTenant,
    ICurrentOrganization currentOrganization,
    ICorrelationIdProvider correlationIdProvider,
    IGuidGenerator guidGenerator,
    RealtimeStopTimeProcessingManager processingManager,
    RealtimeServiceAlertManager serviceAlertManager,
    INotificationLogger notificationLogger) : ITransientDependency
{
    // Kaynagin kapsaminda, aralik dolduysa akisi ceker.
    public virtual async Task FetchAsync(RealtimeFeedTargetModel target, CancellationToken cancellationToken = default)
    {
        using var tenantScope = currentTenant.Change(target.TenantId);
        using var organizationScope = currentOrganization.Change(target.OrganizationId);
        if (!await TryMarkDueAsync(target, cancellationToken))
            return;

        var correlationId = guidGenerator.Create().ToString("N");
        using var correlationScope = correlationIdProvider.Change(correlationId);
        var message = await reader.ReadAsync(target.SourceName, target.Url, cancellationToken);
        if (target.FeedType == RealtimeFeedType.VehiclePositions)
            await StreamAsync(target, await processingManager.ProcessVehiclePositionsAsync(
                target.SourceId, message, correlationId, cancellationToken), cancellationToken);
        else if (target.FeedType == RealtimeFeedType.TripUpdates)
            await StreamAsync(target, await processingManager.ProcessTripUpdatesAsync(
                target.SourceId, message, correlationId, cancellationToken), cancellationToken);
        else if (target.FeedType == RealtimeFeedType.ServiceAlerts)
            await StreamAsync(target, await serviceAlertManager.ProcessAsync(message, cancellationToken), cancellationToken);
    }

    // Islenmis listeyi organizasyona canli akisla iletir. Bos tam liste de gider ki istemci elindekini temizlesin; bayat akista
    // ya da degisiklik yokken bos kismi liste gitmez.
    private Task StreamAsync<TEntity>(RealtimeFeedTargetModel target, RealtimeStreamModel<TEntity> stream,
        CancellationToken cancellationToken = default)
        => stream is { IsFullDataset: false, Entities.Count: 0 }
            ? Task.CompletedTask
            : notificationLogger.StreamAsync(target.FeedType.ToString(), stream, cancellationToken);

    // Akisin araligi icindeki ilk cagriyi isaretler; isaret durdukca ayni akis yeniden cekilmez, cekme hata verse de beklenir.
    private async Task<bool> TryMarkDueAsync(
        RealtimeFeedTargetModel target, CancellationToken cancellationToken = default)
    {
        var key = $"{target.SourceId}:{target.FeedType}";
        if (await fetchCache.GetAsync(key, token: cancellationToken) is not null)
            return false;

        var intervalSeconds = await settingProvider.GetAsync<int>(target.IntervalSettingName);
        await fetchCache.SetAsync(
            key,
            new RealtimeFeedFetchCacheItem { FetchedAt = clock.Now },
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(intervalSeconds) },
            token: cancellationToken);
        return true;
    }
}
