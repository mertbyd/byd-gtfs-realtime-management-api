using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.VehicleStreams;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;
using Byd.LogManagement.Consts;
using Byd.LogManagement.Dtos.Notifications;
using Byd.LogManagement.Interfaces.Notifications;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Json;
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
// ayni kimligi tasir. Arac konumu ve sefer guncellemesi akislari isleme adimina verilir; araclarin son durumu log
// modulunun web bildirimiyle organizasyona itilir, istemci istek atmaz.
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
    INotificationLogger notificationLogger,
    IJsonSerializer jsonSerializer) : ITransientDependency
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
            await NotifyVehiclesAsync(target, await processingManager.ProcessVehiclePositionsAsync(
                target.SourceId, message, correlationId, cancellationToken), cancellationToken);
        else if (target.FeedType == RealtimeFeedType.TripUpdates)
            await processingManager.ProcessTripUpdatesAsync(target.SourceId, message, correlationId, cancellationToken);
    }

    // Araclarin son durumunu organizasyona yayin bildirimiyle iletir; bayat akista liste bos oldugu icin bir sey gitmez.
    private Task NotifyVehiclesAsync(RealtimeFeedTargetModel target, List<RealtimeVehicleStreamModel> vehicles,
        CancellationToken cancellationToken = default)
    {
        if (vehicles.Count == 0)
            return Task.CompletedTask;

        var json = jsonSerializer.Serialize(vehicles);
        return notificationLogger.NotifyAsync(new NotificationRecord(
            [NotificationConsts.WebChannelCode], VehicleStreamConsts.NotificationTitle,
            new Dictionary<string, string>
            {
                [VehicleStreamConsts.TurkishLanguageCode] = json,
                [VehicleStreamConsts.EnglishLanguageCode] = json
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
