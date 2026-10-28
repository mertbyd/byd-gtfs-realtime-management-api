using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeStreams;
using Byd.GtfsRealtimeManagement.Settings;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace Byd.GtfsRealtimeManagement.Managers.RealtimeStreams;

// islevi: Akisin ve kayitlarin guncelligini olcer; sefer tahminlerini GTFS-RT incrementality kuralina gore tam liste ya da
// yalniz degisenler olarak hazirlar.
// sistemdeki gorevi: Tam liste belirli aralikla gider ki sonradan baglanan istemci de butun seferleri alsin; aradaki
// cekmelerde yalniz tahmini degisen ya da yeni seferler ve akistan dusen seferler (silindi isaretiyle) gider. Bildirim
// boyutu boyle kuculur. Eski veri guncel gibi gosterilmez: bayat akis hic islenmez, eski olcumlu kayit akisa girmez.
public class RealtimeStreamManager(ISettingProvider settingProvider) : DomainService
{
    // Baslik zamani yoksa ya da ayardaki esikten eskiyse akis bayattir; islenmez ve gosterilmez.
    public virtual async Task<bool> IsStaleAsync(FeedHeaderModel header, CancellationToken cancellationToken = default)
    {
        var freshSince = await GetFreshSinceAsync(cancellationToken);
        var stale = header.Timestamp is not { } timestamp
                    || DateTimeOffset.FromUnixTimeSeconds((long)timestamp).UtcDateTime < freshSince;
        if (stale)
            Logger.LogWarning("Realtime feed skipped as stale: header timestamp {Timestamp}.", header.Timestamp);
        return stale;
    }

    // Bu andan once olculmus kayit guncel sayilmaz ve akisa girmez; esik bayat akis esigiyle aynidir.
    public virtual async Task<DateTime> GetFreshSinceAsync(CancellationToken cancellationToken = default)
        => Clock.Now.ToUniversalTime().AddSeconds(
            -await settingProvider.GetAsync<int>(GtfsRealtimeManagementSettings.Processing.StaleFeedSeconds));

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
