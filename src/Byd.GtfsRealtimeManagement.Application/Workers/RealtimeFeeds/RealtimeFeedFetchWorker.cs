using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Executors;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSources;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;

namespace Byd.GtfsRealtimeManagement.Workers.RealtimeFeeds;

/// <summary>Aktif kaynaklarin GTFS-RT akislarini organizasyonun ayarindaki aralikla ceker.</summary>
public class RealtimeFeedFetchWorker : AsyncPeriodicBackgroundWorkerBase
{
    public RealtimeFeedFetchWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromSeconds(RealtimeFeedConsts.WorkerPeriodSeconds).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var cancellationToken = workerContext.CancellationToken;
        await using var fetchLock = await services.GetRequiredService<IAbpDistributedLock>()
            .TryAcquireAsync(RealtimeFeedConsts.FetchLockName, cancellationToken: cancellationToken);
        if (fetchLock is null)
            return;
        var targets = await services.GetRequiredService<RealtimeSourceManager>().GetFeedTargetsAsync(cancellationToken);
        var executor = services.GetRequiredService<RealtimeFeedFetchExecutor>();
        foreach (var target in targets)
            await executor.FetchAsync(target, cancellationToken);
    }
}
