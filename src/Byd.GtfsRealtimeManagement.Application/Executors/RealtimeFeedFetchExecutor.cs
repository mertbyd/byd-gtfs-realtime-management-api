using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Timing;

namespace Byd.GtfsRealtimeManagement.Executors;

/// <summary>
/// Bir kaynagin tek akisini kaynagin kapsaminda ve araligi dolduysa ceker.
/// </summary>
// islevi: Hedefin tenant ve organizasyon kapsamina girer; araligi dolmus akisi isaretleyip okuyucuya ceker.
// sistemdeki gorevi: Cekme iscisinin her hedef icin calistirdigi adimdir. Aralik organizasyon ayarindan, cagrinin kaydi ve kapsami
// log modulunde bu kapsamdan yazilir. Arac konumu akisinda kayitli olmayan araclar icin arac modulunden kayit istenir.
public class RealtimeFeedFetchExecutor(
    IRealtimeFeedReader reader,
    ISettingProvider settingProvider,
    IDistributedCache<RealtimeFeedFetchCacheItem> fetchCache,
    IClock clock,
    ICurrentTenant currentTenant,
    ICurrentOrganization currentOrganization,
    RealtimeSnapshotManager snapshotManager) : ITransientDependency
{
    // Kaynagin kapsaminda, aralik dolduysa akisi ceker.
    public virtual async Task FetchAsync(RealtimeFeedTargetModel target, CancellationToken cancellationToken = default)
    {
        using var tenantScope = currentTenant.Change(target.TenantId);
        using var organizationScope = currentOrganization.Change(target.OrganizationId);
        if (!await TryMarkDueAsync(target, cancellationToken))
            return;

        var message = await reader.ReadAsync(target.SourceName, target.Url, cancellationToken);
        if (target.FeedType == RealtimeFeedType.VehiclePositions)
            await snapshotManager.RequestUnregisteredVehiclesAsync(message, cancellationToken);
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
