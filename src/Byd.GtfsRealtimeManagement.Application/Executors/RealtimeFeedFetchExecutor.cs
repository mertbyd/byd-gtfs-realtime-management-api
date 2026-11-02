using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.ExceptionCodes;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Managers.RealtimeServiceAlerts;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStreams;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeStreams;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;
using Byd.LogManagement.Consts;
using Byd.LogManagement.Dtos.Notifications;
using Byd.LogManagement.Interfaces.Notifications;
using Volo.Abp;
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
// islevi: Hedefin tenant ve organizasyon kapsamina girer; araligi dolmus akisi isaretleyip okuyucuya kosullu istekle ceker,
// isleme adimina verir ve akisin sagligi degistiyse organizasyona bildirir.
// sistemdeki gorevi: Cekme iscisinin her hedef icin calistirdigi adimdir. Aralik organizasyon ayarindan, cagrinin kaydi ve kapsami
// log modulunde bu kapsamdan yazilir. Her cekme kendi correlation kimligiyle yapilir; ham kayit ve islenmis durak satiri
// ayni kimligi tasir. Her akis kendi isleme adimina verilir; araclarin son durumu, seferlerin tahmini durak zamanlari ve
// gecerli servis uyarilari log modulunun canli akisiyla, akis turunun adini tasiyarak organizasyona itilir. Canli akis kalici
// degildir: olay kutusuna ve bildirim tablosuna yazilmaz, kacan mesajin yerini bir sonraki alir; istemci istek atmaz.
// Kaynaga ulasilamamasi, cozulemeyen yanit, bayat akis ve baska veri setine ait akis ise kalici bildirimle ve yalniz durum
// degistiginde (sorun basladiginda ve gectiginde) organizasyona gider; bir kaynagin hatasi diger kaynaklarin cekmesini durdurmaz.
public class RealtimeFeedFetchExecutor(
    IRealtimeFeedReader reader,
    ISettingProvider settingProvider,
    IDistributedCache<RealtimeFeedFetchCacheItem> fetchCache,
    IClock clock,
    ICurrentTenant currentTenant,
    ICurrentOrganization currentOrganization,
    ICorrelationIdProvider correlationIdProvider,
    IGuidGenerator guidGenerator,
    RealtimeSnapshotManager snapshotManager,
    RealtimeStreamManager streamManager,
    RealtimeStopTimeProcessingManager processingManager,
    RealtimeServiceAlertManager serviceAlertManager,
    INotificationLogger notificationLogger) : ITransientDependency
{
    // Kaynagin kapsaminda, aralik dolduysa akisi ceker, isler ve sagligini bildirir.
    public virtual async Task FetchAsync(RealtimeFeedTargetModel target, CancellationToken cancellationToken = default)
    {
        using var tenantScope = currentTenant.Change(target.TenantId);
        using var organizationScope = currentOrganization.Change(target.OrganizationId);
        if (!await TryMarkDueAsync(target, cancellationToken))
            return;

        var correlationId = guidGenerator.Create().ToString("N");
        using var correlationScope = correlationIdProvider.Change(correlationId);
        var health = await ReadAndProcessAsync(target, correlationId, cancellationToken);
        await NotifyHealthAsync(target, health, cancellationToken);
    }

    // Akisi kosullu istekle okur ve isler; ulasilamayan kaynak ve cozulemeyen yanit sagliga cevrilir. Icerik degismediyse
    // yalniz guncelligi olculur.
    private async Task<RealtimeFeedHealth> ReadAndProcessAsync(RealtimeFeedTargetModel target, string correlationId,
        CancellationToken cancellationToken = default)
    {
        FeedMessageModel? message;
        try
        {
            message = await reader.ReadIfChangedAsync(target.SourceName, target.Url, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return RealtimeFeedHealth.Unreachable;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return RealtimeFeedHealth.Unreachable;
        }
        catch (BusinessException exception) when (exception.Code is RealtimeFeedExceptionCodes.FeedInvalid
                                                      or RealtimeFeedExceptionCodes.FeedNotSupported)
        {
            return RealtimeFeedHealth.Invalid;
        }
        return message is null
            ? await streamManager.GetUnchangedHealthAsync(
                await snapshotManager.EnsureLoadedAsync(cancellationToken), target.FeedType, cancellationToken)
            : await ProcessAsync(target, message, correlationId, cancellationToken);
    }

    // Akisi turunun isleme adimina verir ve sonucunu canli akisla iletir.
    private async Task<RealtimeFeedHealth> ProcessAsync(RealtimeFeedTargetModel target, FeedMessageModel message,
        string correlationId, CancellationToken cancellationToken = default) => target.FeedType switch
    {
        RealtimeFeedType.VehiclePositions => await StreamAsync(target, await processingManager.ProcessVehiclePositionsAsync(
            target.SourceId, message, correlationId, cancellationToken), cancellationToken),
        RealtimeFeedType.TripUpdates => await StreamAsync(target, await processingManager.ProcessTripUpdatesAsync(
            target.SourceId, message, correlationId, cancellationToken), cancellationToken),
        _ => await StreamAsync(target, await serviceAlertManager.ProcessAsync(message, cancellationToken), cancellationToken)
    };

    // Islenmis listeyi organizasyona canli akisla iletir ve akisin sagligini dondurur. Bos tam liste de gider ki istemci
    // elindekini temizlesin; sagliksiz akista ya da degisiklik yokken bos kismi liste gitmez.
    private async Task<RealtimeFeedHealth> StreamAsync<TEntity>(RealtimeFeedTargetModel target,
        RealtimeStreamModel<TEntity> stream, CancellationToken cancellationToken = default)
    {
        if (stream is not { IsFullDataset: false, Entities.Count: 0 })
            await notificationLogger.StreamAsync(target.FeedType.ToString(), stream, cancellationToken);
        return stream.Health;
    }

    // Akisin sagligi oncekinden farkliysa organizasyonun ekranlarina kalici bildirim gonderir. Baslik saglik durumunun
    // adidir, istemci onu kendi dilinde gosterir; metin kaynak ve akis adidir.
    private async Task NotifyHealthAsync(RealtimeFeedTargetModel target, RealtimeFeedHealth health,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await snapshotManager.EnsureLoadedAsync(cancellationToken);
        if (!streamManager.ChangeHealth(snapshot, target.FeedType, health))
            return;
        var text = $"{target.SourceName} {target.FeedType}";
        await notificationLogger.NotifyAsync(new NotificationRecord(
            [NotificationConsts.WebChannelCode], health.ToString(),
            new Dictionary<string, string>
            {
                [RealtimeFeedConsts.TurkishLanguageCode] = text,
                [RealtimeFeedConsts.EnglishLanguageCode] = text
            })
        {
            ExternalId = target.SourceId,
            IsTenantNotification = true
        }, cancellationToken);
    }

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
