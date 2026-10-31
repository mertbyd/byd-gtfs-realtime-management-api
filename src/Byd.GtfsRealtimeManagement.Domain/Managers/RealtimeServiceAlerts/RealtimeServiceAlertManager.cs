using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStreams;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeStreams;
using Volo.Abp.Domain.Services;

namespace Byd.GtfsRealtimeManagement.Managers.RealtimeServiceAlerts;

// islevi: Servis uyarilarini plan kopyasiyla dogrular; gosterim araliginda olan ve kapsaminda planda karsiligi bulunan
// uyarilari istemciye gidecek bicime cevirir.
// sistemdeki gorevi: Uyarilarin isleme adimidir. Uyari sayisi az ve her cekmede tam liste geldigi icin akis her seferinde
// tam liste olarak gider; suresi dolan ya da akistan dusen uyari bir sonraki listede yer almaz. Gecersiz uyari sayisi
// cekme basina log'a yazilir.
public class RealtimeServiceAlertManager(
    RealtimeSnapshotManager snapshotManager,
    RealtimeStreamManager streamManager) : DomainService
{
    // Akistaki uyarilardan gosterilecekleri tam liste olarak dondurur; bayat akis ya da plan yoksa liste bos ve kismidir.
    public virtual async Task<RealtimeStreamModel<RealtimeServiceAlertStreamModel>> ProcessAsync(
        FeedMessageModel message, CancellationToken cancellationToken = default)
    {
        var snapshot = await snapshotManager.EnsureLoadedAsync(cancellationToken);
        if (snapshot.Plan is null || await streamManager.IsStaleAsync(message.Header, cancellationToken))
            return new RealtimeStreamModel<RealtimeServiceAlertStreamModel>();
        var now = (ulong)Clock.Now.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds;
        var entities = message.Entity.Where(entity => entity.Alert is not null).ToList();
        var alerts = entities.Select(entity => ToStream(snapshot.Plan, entity.Id, entity.Alert!, now))
            .OfType<RealtimeServiceAlertStreamModel>().ToList();
        Logger.LogInformation(
            "Realtime service alerts processed: {Alerts} alerts, {Shown} shown, {Hidden} expired or not matched to the plan.",
            entities.Count, alerts.Count, entities.Count - alerts.Count);
        return new RealtimeStreamModel<RealtimeServiceAlertStreamModel> { IsFullDataset = true, Entities = alerts };
    }

    // Uyari gosterim araligindaysa ve kapsaminda planda karsiligi olan en az bir secici varsa uyariyi kurar. Gosterim araligi
    // communication_period'dir; yoksa kullanimdan kalkan active_period okunur.
    private RealtimeServiceAlertStreamModel? ToStream(RealtimePlanSnapshotModel plan, string alertId, AlertModel alert,
        ulong now)
    {
        var periods = alert.CommunicationPeriod.Count > 0 ? alert.CommunicationPeriod : alert.ActivePeriod;
        var entities = alert.InformedEntity.Where(selector => snapshotManager.IsInPlan(plan, selector)).ToList();
        if (entities.Count == 0 || !IsActive(periods, now))
            return null;
        return new RealtimeServiceAlertStreamModel
        {
            AlertId = alertId,
            Cause = alert.Cause ?? ServiceAlertConsts.UnknownCauseCode,
            Effect = alert.Effect ?? ServiceAlertConsts.UnknownEffectCode,
            SeverityLevel = alert.SeverityLevel ?? ServiceAlertConsts.UnknownSeverityCode,
            CommunicationPeriods = periods, ImpactPeriods = alert.ImpactPeriod, InformedEntities = entities,
            HeaderText = alert.HeaderText, DescriptionText = alert.DescriptionText, Url = alert.Url,
            CauseDetail = alert.CauseDetail, EffectDetail = alert.EffectDetail
        };
    }

    // GTFS-RT TimeRange: bos uc o yonde aciktir; hic aralik yoksa uyari surekli gecerlidir.
    private static bool IsActive(List<TimeRangeModel> periods, ulong now)
        => periods.Count == 0 || periods.Any(period => (period.Start ?? 0) <= now && now <= (period.End ?? ulong.MaxValue));
}
