using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeStreams;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Settings;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace Byd.GtfsRealtimeManagement.Managers.RealtimeStreams;

// islevi: Akisin sagligini (guncellik ve veri seti uyumu) ve kayitlarin guncelligini olcer; ayni yaniti ikinci kez
// isletmez; sefer tahminlerini GTFS-RT incrementality kuralina gore tam liste ya da yalniz degisenler olarak hazirlar.
// sistemdeki gorevi: Tam liste belirli aralikla gider ki sonradan baglanan istemci de butun seferleri alsin; aradaki
// cekmelerde yalniz tahmini degisen ya da yeni seferler ve akistan dusen seferler (silindi isaretiyle) gider. Bildirim
// boyutu boyle kuculur. Eski veri guncel gibi gosterilmez: bayat ya da baska veri setine ait akis hic islenmez, eski
// olcumlu kayit akisa girmez. Saglik degisimi cekme adiminda organizasyona bildirilir.
public class RealtimeStreamManager(
    ISettingProvider settingProvider,
    RealtimeSnapshotManager snapshotManager) : DomainService
{
    // Akisin islenip islenemeyecegini soyler: bayat akis Stale, organizasyonun aktif veri setine ait olmayan akis
    // PlanMismatch; plan yoksa akis plana gore degerlendirilmez.
    public virtual async Task<RealtimeFeedHealth> GetHealthAsync(RealtimeOrganizationSnapshotModel snapshot,
        FeedMessageModel message, RealtimeFeedType feedType, CancellationToken cancellationToken = default)
    {
        if (await IsStaleAsync(message.Header, feedType, cancellationToken))
            return RealtimeFeedHealth.Stale;
        return snapshot.Plan is { } plan && !snapshotManager.IsSameDataset(plan, message)
            ? RealtimeFeedHealth.PlanMismatch
            : RealtimeFeedHealth.Healthy;
    }

    // Icerik degismedigi icin (HTTP 304) yanitsiz gecen cekmede akis, son islenen baslik zamani esikten eskiyse bayattir;
    // uygulama yeniden basladiktan sonra henuz islenen yanit yoksa onceki saglik korunur.
    public virtual async Task<RealtimeFeedHealth> GetUnchangedHealthAsync(RealtimeOrganizationSnapshotModel snapshot,
        RealtimeFeedType feedType, CancellationToken cancellationToken = default)
    {
        if (!snapshot.ProcessedHeaderTimestamps.TryGetValue(feedType, out var timestamp))
            return snapshot.FeedHealth.GetValueOrDefault(feedType);
        return ToUtc(timestamp) < await GetFreshSinceAsync(feedType, cancellationToken)
            ? RealtimeFeedHealth.Stale
            : RealtimeFeedHealth.Healthy;
    }

    // Baslik zamani son islenenden yeni degilse yanit tekrardir ve islenmez (GTFS-RT: baslik zamani icerik degistikce
    // degisir, geri gitmez); yeniyse islendi olarak isaretlenir.
    public virtual bool IsRepeated(RealtimeOrganizationSnapshotModel snapshot, RealtimeFeedType feedType,
        FeedHeaderModel header)
    {
        if (snapshot.ProcessedHeaderTimestamps.TryGetValue(feedType, out var processed) && processed >= header.Timestamp)
            return true;
        MarkProcessed(snapshot, feedType, header);
        return false;
    }

    // Islenen yanitin baslik zamanini kopyaya yazar; icerik degismedigi cekmede bayatlik bu zamandan olculur.
    public virtual void MarkProcessed(RealtimeOrganizationSnapshotModel snapshot, RealtimeFeedType feedType,
        FeedHeaderModel header)
        => snapshot.ProcessedHeaderTimestamps[feedType] = header.Timestamp!.Value;

    // Akisin sagligini kopyaya yazar; oncekinden farkliysa true doner. Konum akisi sagliksizsa izleme kesilir, yapilmayan
    // sefer karari yalniz kesintisiz izlenen seferlere verilir.
    public virtual bool ChangeHealth(RealtimeOrganizationSnapshotModel snapshot, RealtimeFeedType feedType,
        RealtimeFeedHealth health)
    {
        if (feedType == RealtimeFeedType.VehiclePositions && health != RealtimeFeedHealth.Healthy)
            snapshot.WatchedSince = null;
        var previous = snapshot.FeedHealth.GetValueOrDefault(feedType);
        snapshot.FeedHealth[feedType] = health;
        return previous != health;
    }

    // Bu andan once olculmus kayit guncel sayilmaz ve akisa girmez; esik akis turunun bayatlik esigidir.
    public virtual async Task<DateTime> GetFreshSinceAsync(RealtimeFeedType feedType,
        CancellationToken cancellationToken = default)
        => Clock.Now.ToUniversalTime().AddSeconds(-await settingProvider.GetAsync<int>(
            feedType == RealtimeFeedType.ServiceAlerts
                ? GtfsRealtimeManagementSettings.Processing.ServiceAlertsStaleFeedSeconds
                : GtfsRealtimeManagementSettings.Processing.StaleFeedSeconds));

    // Baslik zamani yoksa ya da akis turunun esiginden eskiyse akis bayattir; islenmez ve gosterilmez.
    private async Task<bool> IsStaleAsync(FeedHeaderModel header, RealtimeFeedType feedType,
        CancellationToken cancellationToken = default)
    {
        var freshSince = await GetFreshSinceAsync(feedType, cancellationToken);
        var stale = header.Timestamp is not { } timestamp || ToUtc(timestamp) < freshSince;
        if (stale)
            Logger.LogWarning("Realtime {FeedType} feed skipped as stale: header timestamp {Timestamp}.", feedType,
                header.Timestamp);
        return stale;
    }

    private static DateTime ToUtc(ulong unixSeconds) => DateTimeOffset.FromUnixTimeSeconds((long)unixSeconds).UtcDateTime;

    // Araligi dolduysa butun seferleri, dolmadiysa son gonderimden beri degisenleri dondurur; gonderilen durumu kopyaya yazar.
    public virtual RealtimeStreamModel<RealtimeTripUpdateStreamModel> ToTripUpdateStream(
        RealtimeOrganizationSnapshotModel snapshot, List<RealtimeTripUpdateStreamModel> trips)
    {
        var now = Clock.Now.ToUniversalTime();
        var isFullDataset = snapshot.TripUpdatesFullDatasetSentAt is not { } sentAt
            || now - sentAt >= TimeSpan.FromSeconds(RealtimeStreamConsts.TripUpdatesFullDatasetSeconds);
        var current = trips.GroupBy(GetKey).ToDictionary(group => group.Key, group => group.Last());
        var entities = isFullDataset ? current.Values.ToList() : GetChanges(snapshot, current);
        snapshot.SentTripUpdates.Clear();
        foreach (var pair in current)
            snapshot.SentTripUpdates[pair.Key] = pair.Value;
        if (isFullDataset)
            snapshot.TripUpdatesFullDatasetSentAt = now;
        return new RealtimeStreamModel<RealtimeTripUpdateStreamModel> { IsFullDataset = isFullDataset, Entities = entities };
    }

    // Yeni ya da tahmini degisen seferler ile son gonderimde olup akistan dusen seferler; dusen sefer silindi isaretlenir.
    private static List<RealtimeTripUpdateStreamModel> GetChanges(RealtimeOrganizationSnapshotModel snapshot,
        Dictionary<string, RealtimeTripUpdateStreamModel> current)
    {
        var changes = current
            .Where(pair => !snapshot.SentTripUpdates.TryGetValue(pair.Key, out var sent)
                           || GetSignature(sent) != GetSignature(pair.Value))
            .Select(pair => pair.Value)
            .ToList();
        foreach (var removed in snapshot.SentTripUpdates.Where(pair => !current.ContainsKey(pair.Key)).Select(pair => pair.Value))
        {
            (removed.IsDeleted, removed.Stops) = (true, []);
            changes.Add(removed);
        }
        return changes;
    }

    private static string GetKey(RealtimeTripUpdateStreamModel trip)
        => $"{trip.GtfsTripId}|{trip.ServiceDate:yyyyMMdd}|{trip.StartTime}";

    // Olcum ani disinda istemcinin gordugu her alan; imza degismediyse sefer yeniden gonderilmez.
    private static string GetSignature(RealtimeTripUpdateStreamModel trip)
        => string.Join(';', trip.Stops.Select(stop =>
                $"{stop.StopSequence}:{stop.StopScheduleRelationshipCode}:{stop.ArrivalTime:O}:{stop.DepartureTime:O}"))
           + $"|{trip.TripScheduleRelationshipCode}|{trip.Delay}|{trip.VehicleId}|{trip.GtfsVehicleId}";
}
