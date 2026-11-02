using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStopTimes;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;

namespace Byd.GtfsRealtimeManagement.Workers.RealtimeStopTimes;

/// <summary>Saklama suresi dolan islenmis durak satirlarini siler.</summary>
// islevi: Her turda servis gunu saklama suresinden eski durak satirlarini butun kaynaklarda siler.
// sistemdeki gorevi: Islenmis modelin tablosunu sinirli tutar; bir instance'in calismasi kilitle saglanir.
public class RealtimeStopTimeRetentionWorker : AsyncPeriodicBackgroundWorkerBase
{
    public RealtimeStopTimeRetentionWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromSeconds(RealtimeStopTimeConsts.RetentionScanPeriodSeconds).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var cancellationToken = workerContext.CancellationToken;
        await using var retentionLock = await services.GetRequiredService<IAbpDistributedLock>()
            .TryAcquireAsync(RealtimeStopTimeConsts.RetentionLockName, cancellationToken: cancellationToken);
        if (retentionLock is null)
            return;
        await services.GetRequiredService<RealtimeStopTimeManager>().DeleteExpiredAsync(cancellationToken);
    }
}
