using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Managers.RealtimeStreams;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Models.RealtimeStreams;
using Byd.GtfsRealtimeManagement.Settings;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace Byd.GtfsRealtimeManagement.Managers.RealtimeStopTimes;

// islevi: Arac konumu ve sefer guncellemesi akislarini plan kopyasiyla dogrular; aracin duraga girisini, cikisini,
// durmadan gecisini ve sefer/durak durumunu bulup durak satirlarina yazar.
// sistemdeki gorevi: Islenmis modelin (RealtimeStopTime) yazaridir; her degisiklik kendisini kanitlayan cekmenin correlation
// kimligini tasir. Araclarin son durumunu ve seferlerin tahmini durak zamanlarini bildirimle gitmek uzere dondurur. Bayat
// akis islenmez; gecersiz kayit atlanir ve cekme basina ozet log'a yazilir.
public class RealtimeStopTimeProcessingManager(
    RealtimeSnapshotManager snapshotManager,
    RealtimeStopTimeManager stopTimeManager,
    RealtimeStreamManager streamManager,
    IRealtimeStopTimeRepository repository,
    IRealtimeLookupRepository<TripScheduleRelationship> tripRelationshipRepository,
    IRealtimeLookupRepository<StopScheduleRelationship> stopRelationshipRepository,
    IRealtimeLookupRepository<VehicleStopStatus> vehicleStopStatusRepository,
    IRealtimeLookupRepository<ViolationType> violationTypeRepository,
    ISettingProvider settingProvider,
    IUnitOfWorkManager unitOfWorkManager) : DomainService
{
    private const double EarthRadiusMeters = 6_371_000;

    // Yaricap kontrolu aracin gectigi son duraktan sonraki bu kadar durakla sinirlidir; GPS atlamasi tolere edilir.
    private const int LookAheadStops = 3;

    // Kayitsiz araclar icin kayit ister; her aracin seferindeki duraga giris ve cikisini bulup satirlara yazar ve araclarin
    // akisla gidecek son durumunu dondurur. Bayat akis islenmez ve gosterilmez, konumu eski olculmus arac listeye girmez; plan
    // yoksa araclar yalniz konumla doner.
    public virtual async Task<RealtimeStreamModel<RealtimeVehicleStreamModel>> ProcessVehiclePositionsAsync(
        Guid realtimeSourceId,
        FeedMessageModel message, string correlationId, CancellationToken cancellationToken = default)
    {
        await snapshotManager.RequestUnregisteredVehiclesAsync(message, cancellationToken);
        var snapshot = await snapshotManager.EnsureLoadedAsync(cancellationToken);
        if (await streamManager.IsStaleAsync(message.Header, cancellationToken))
            return new RealtimeStreamModel<RealtimeVehicleStreamModel>();
        var positions = message.Entity.Select(entity => entity.Vehicle).OfType<VehiclePositionModel>().ToList();
        if (snapshot.Plan is not null)
            await TrackPositionsAsync(realtimeSourceId, snapshot, positions, correlationId, cancellationToken);
        var freshSince = await streamManager.GetFreshSinceAsync(cancellationToken);
        return new RealtimeStreamModel<RealtimeVehicleStreamModel>
        {
            IsFullDataset = true,
            Entities = positions.Select(position => ToStream(snapshot, position)).OfType<RealtimeVehicleStreamModel>()
                .Where(vehicle => vehicle.MeasuredAt is not { } measuredAt || measuredAt >= freshSince).ToList()
        };
    }

    // Konumlari plandaki duraklarla karsilastirip degisiklikleri yazar; eslesmeyen kayit sayisi log'a yazilir.
    private async Task TrackPositionsAsync(Guid realtimeSourceId, RealtimeOrganizationSnapshotModel snapshot,
        List<VehiclePositionModel> positions, string correlationId, CancellationToken cancellationToken = default)
    {
        var radius = await settingProvider.GetAsync<double>(GtfsRealtimeManagementSettings.Processing.StopRadiusMeters);
        var stoppedSpeed = await settingProvider.GetAsync<double>(
            GtfsRealtimeManagementSettings.Processing.StoppedSpeedMetersPerSecond);
        var changes = new List<RealtimeStopTimeChangeModel>();
        var invalid = positions.Count(position =>
            !TrackPosition(snapshot, position, radius, stoppedSpeed, correlationId, changes));
        await MarkDepartureViolationsAsync(changes, cancellationToken);
        await PersistAsync(realtimeSourceId, snapshot, changes, cancellationToken);
        Logger.LogInformation(
            "Realtime vehicle positions processed: {Positions} positions, {Changes} stop changes, {Invalid} not matched to the plan.",
            positions.Count, changes.Count, invalid);
    }

    // Sefer ve durak planlama durumunu ve gecikmeyi gunceller; iptal/silinen sefer ve atlanan durak satira yazilir. Plandaki
    // seferlerin tahmini durak zamanlarini tam liste ya da yalniz degisenler olarak akisla gitmek uzere dondurur; bayat akis
    // islenmez ve gosterilmez, guncellemesi eski olculmus sefer listeye girmez.
    public virtual async Task<RealtimeStreamModel<RealtimeTripUpdateStreamModel>> ProcessTripUpdatesAsync(
        Guid realtimeSourceId, FeedMessageModel message, string correlationId, CancellationToken cancellationToken = default)
    {
        var snapshot = await snapshotManager.EnsureLoadedAsync(cancellationToken);
        if (snapshot.Plan is null || await streamManager.IsStaleAsync(message.Header, cancellationToken))
            return new RealtimeStreamModel<RealtimeTripUpdateStreamModel>();
        var measuredAt = ToUtc((long)message.Header.Timestamp!.Value);
        RemoveExpiredStatuses(snapshot, measuredAt);
        var updates = message.Entity.Select(entity => entity.TripUpdate).OfType<TripUpdateModel>().ToList();
        var changes = new List<RealtimeStopTimeChangeModel>();
        var trips = updates.Select(update => TrackTripUpdate(snapshot, update, measuredAt, correlationId, changes))
            .OfType<RealtimeTripUpdateStreamModel>().ToList();
        await PersistAsync(realtimeSourceId, snapshot, changes, cancellationToken);
        Logger.LogInformation(
            "Realtime trip updates processed: {Updates} trips, {Changes} status changes, {Invalid} not matched to the plan.",
            updates.Count, changes.Count, updates.Count - trips.Count);
        var freshSince = await streamManager.GetFreshSinceAsync(cancellationToken);
        return streamManager.ToTripUpdateStream(snapshot, trips.Where(trip => trip.MeasuredAt >= freshSince).ToList());
    }

    // Aracin bildirimle gidecek durumunu kurar: konum, plandaki seferi ve hatti, bulundugu ve siradaki durak, siradaki durak icin
    // bildirilen gecikme. Arac kimligi ya da konumu yoksa arac gosterilmez.
    private RealtimeVehicleStreamModel? ToStream(RealtimeOrganizationSnapshotModel snapshot, VehiclePositionModel position)
    {
        if (position.Vehicle?.Id is not { Length: > 0 } gtfsVehicleId || position.Position is not { } point)
            return null;
        DateTime? measuredAt = position.Timestamp is { } timestamp
            ? DateTimeOffset.FromUnixTimeSeconds((long)timestamp).UtcDateTime : null;
        var instance = snapshot.Plan is { } plan && position.Trip is { } trip && measuredAt is { } at
            ? snapshotManager.ResolveTripInstance(plan, trip, at) : null;
        var visit = snapshot.VehicleVisits.GetValueOrDefault(gtfsVehicleId);
        var current = instance?.Trip.StopTimes.FirstOrDefault(stop => stop.StopSequence == visit?.CurrentSequence);
        var next = instance?.Trip.StopTimes.FirstOrDefault(stop =>
            stop.StopSequence > (visit?.CurrentSequence ?? visit?.PassedSequence ?? -1));
        return new RealtimeVehicleStreamModel
        {
            VehicleId = snapshot.VehicleIds.TryGetValue(gtfsVehicleId, out var vehicleId) ? vehicleId : null,
            GtfsVehicleId = gtfsVehicleId, Latitude = point.Latitude, Longitude = point.Longitude,
            Bearing = point.Bearing, Speed = point.Speed, MeasuredAt = measuredAt,
            GtfsTripId = instance?.Trip.GtfsTripId ?? position.Trip?.TripId, GtfsRouteId = instance?.Trip.GtfsRouteId,
            ServiceDate = instance?.ServiceDate, CurrentStopSequence = current?.StopSequence,
            CurrentGtfsStopId = current?.GtfsStopId, IsStopped = current is not null && visit!.Stopped,
            NextStopSequence = next?.StopSequence, NextGtfsStopId = next?.GtfsStopId,
            ReportedDelay = instance is not null && next is not null
                ? snapshot.StopStatuses.GetValueOrDefault(snapshotManager.GetStopKey(instance, next.StopSequence))?.ReportedDelay
                : null
        };
    }

    // Aracin konumunu seferindeki duraklarla karsilastirir; arac, sefer, konum ya da zaman eksikse veya sefer planda o gun
    // calismiyorsa kayit gecersizdir.
    private bool TrackPosition(RealtimeOrganizationSnapshotModel snapshot, VehiclePositionModel position, double radius,
        double stoppedSpeed, string correlationId, List<RealtimeStopTimeChangeModel> changes)
    {
        if (position.Vehicle?.Id is not { Length: > 0 } gtfsVehicleId || position.Trip is null
            || position.Position is not { } point || position.Timestamp is not { } timestamp)
            return false;
        var measuredAt = DateTimeOffset.FromUnixTimeSeconds((long)timestamp).UtcDateTime;
        if (snapshotManager.ResolveTripInstance(snapshot.Plan!, position.Trip, measuredAt) is not { } instance)
            return false;
        var visit = GetVisit(snapshot, gtfsVehicleId, snapshotManager.GetTripKey(instance));
        var template = new RealtimeStopTimeChangeModel
        {
            Instance = instance, CorrelationId = correlationId, GtfsVehicleId = gtfsVehicleId, MeasuredAt = measuredAt,
            Latitude = point.Latitude, Longitude = point.Longitude, VehicleStopStatusCode = position.CurrentStatus
        };
        var speed = (double?)point.Speed ?? DerivedSpeed(visit, point, measuredAt);
        var stop = FindStopInRadius(instance.Trip, visit.PassedSequence, point, radius);
        if (visit.CurrentSequence is { } current && current != stop?.StopSequence)
            Leave(visit, template, current, changes);
        if (stop is not null)
            Enter(visit, template, stop, speed <= stoppedSpeed, changes);
        (visit.LastLatitude, visit.LastLongitude, visit.LastMeasuredAt) = (point.Latitude, point.Longitude, measuredAt);
        return true;
    }

    // Arac yaricapa girdi: arada gorulmeden gecilen duraklar kapanir; arac duruyorsa giris yazilir, durmuyorsa durak gecis
    // olarak izlenir. Ayni durakta durmaya baslayan arac icin giris o an yazilir.
    private static void Enter(RealtimeVehicleVisitModel visit, RealtimeStopTimeChangeModel template,
        RealtimeStopTimeSnapshotModel stop, bool stopped, List<RealtimeStopTimeChangeModel> changes)
    {
        if (visit.CurrentSequence != stop.StopSequence)
        {
            changes.AddRange(template.Instance.Trip.StopTimes
                .Where(unseen => unseen.StopSequence > visit.PassedSequence && unseen.StopSequence < stop.StopSequence)
                .Select(unseen => With(template, unseen, RealtimeStopTimeChangeKind.PassedThrough)));
            (visit.CurrentSequence, visit.Stopped) = (stop.StopSequence, false);
        }
        if (!stopped || visit.Stopped)
            return;
        visit.Stopped = true;
        changes.Add(With(template, stop, RealtimeStopTimeChangeKind.Arrival));
    }

    // Arac duragin yaricapindan cikti ya da baska duraga gecti: durmus araca cikis, durmadan gecene gecis yazilir.
    private static void Leave(RealtimeVehicleVisitModel visit, RealtimeStopTimeChangeModel template, int currentSequence,
        List<RealtimeStopTimeChangeModel> changes)
    {
        var stop = template.Instance.Trip.StopTimes.First(candidate => candidate.StopSequence == currentSequence);
        changes.Add(With(template, stop,
            visit.Stopped ? RealtimeStopTimeChangeKind.Departure : RealtimeStopTimeChangeKind.PassedThrough));
        (visit.PassedSequence, visit.CurrentSequence, visit.Stopped) = (currentSequence, null, false);
    }

    // Seferin duraklarini tahmin eder, her duragin durumunu ve gecikmesini kopyaya yazar ve seferin bildirimle gidecek
    // durumunu kurar. Sefer planda o gun calismiyorsa kayit gecersizdir.
    private RealtimeTripUpdateStreamModel? TrackTripUpdate(RealtimeOrganizationSnapshotModel snapshot, TripUpdateModel update,
        DateTime measuredAt, string correlationId, List<RealtimeStopTimeChangeModel> changes)
    {
        if (snapshotManager.ResolveTripInstance(snapshot.Plan!, update.Trip, measuredAt) is not { } instance)
            return null;
        var tripCode = update.Trip.ScheduleRelationship ?? TripScheduleRelationshipConsts.ScheduledCode;
        var predictions = Predict(instance, update, tripCode);
        foreach (var stop in instance.Trip.StopTimes)
            UpdateStatus(snapshot, instance, stop, tripCode,
                predictions.FirstOrDefault(prediction => prediction.StopSequence == stop.StopSequence), correlationId, changes);
        return new RealtimeTripUpdateStreamModel
        {
            VehicleId = update.Vehicle?.Id is { } gtfsVehicleId && snapshot.VehicleIds.TryGetValue(gtfsVehicleId, out var id)
                ? id : null,
            GtfsVehicleId = update.Vehicle?.Id, GtfsTripId = instance.Trip.GtfsTripId,
            GtfsRouteId = instance.Trip.GtfsRouteId, ServiceDate = instance.ServiceDate, StartTime = instance.StartTime,
            TripScheduleRelationshipCode = tripCode, Delay = update.Delay,
            MeasuredAt = update.Timestamp is { } timestamp ? ToUtc((long)timestamp) : measuredAt, Stops = predictions
        };
    }

    // GTFS-RT yayilim kurali: kaynagin ilk bilgi verdigi duraktan itibaren, bilgisi gelmeyen durak onceki duragin kalkis
    // gecikmesini devralir; atlanan durak gecikmeyi degistirmeden ileri tasir, NO_DATA ise sonraki duraklarin tahminini bir
    // sonraki bilgiye kadar keser. Sefer genel gecikme bildirdiyse ilk duraktan baslanir; iptal/silinen seferde tahmin yoktur.
    private List<RealtimeStopTimePredictionModel> Predict(RealtimeTripInstanceModel instance, TripUpdateModel update,
        int tripCode)
    {
        var predictions = new List<RealtimeStopTimePredictionModel>();
        if (IsTripRemoved(tripCode))
            return predictions;
        var delay = update.Delay;
        var noData = false;
        foreach (var stop in instance.Trip.StopTimes)
        {
            var stopUpdate = FindStopUpdate(update, stop);
            if (stopUpdate is null && predictions.Count == 0 && delay is null)
                continue;
            var prediction = PredictStop(instance, stop, stopUpdate, noData ? null : delay, noData);
            predictions.Add(prediction);
            noData = prediction.StopScheduleRelationshipCode == StopScheduleRelationshipConsts.NoDataCode;
            if (prediction.StopScheduleRelationshipCode != StopScheduleRelationshipConsts.SkippedCode)
                delay = prediction.DepartureDelay;
        }
        return predictions;
    }

    // Duragin tahmini: bildirilen zaman gecikmeden onceliklidir; varis bilgisi yoksa devralinan gecikme, kalkis bilgisi
    // yoksa varis gecikmesi kullanilir. Atlanan ve verisi olmayan durakta zaman ve gecikme bos kalir.
    private RealtimeStopTimePredictionModel PredictStop(RealtimeTripInstanceModel instance,
        RealtimeStopTimeSnapshotModel stop, StopTimeUpdateModel? stopUpdate, int? delay, bool noData)
    {
        var prediction = new RealtimeStopTimePredictionModel
        {
            StopSequence = stop.StopSequence, GtfsStopId = stop.GtfsStopId,
            StopScheduleRelationshipCode = stopUpdate?.ScheduleRelationship
                ?? (noData ? StopScheduleRelationshipConsts.NoDataCode : StopScheduleRelationshipConsts.ScheduledCode)
        };
        if (prediction.StopScheduleRelationshipCode is StopScheduleRelationshipConsts.SkippedCode
            or StopScheduleRelationshipConsts.NoDataCode)
            return prediction;
        var scheduledArrival = snapshotManager.GetScheduledTime(instance, stop.ArrivalTime ?? stop.DepartureTime);
        var scheduledDeparture = snapshotManager.GetScheduledTime(instance, stop.DepartureTime ?? stop.ArrivalTime);
        prediction.ArrivalDelay = EventDelay(stopUpdate?.Arrival, scheduledArrival) ?? delay;
        prediction.DepartureDelay = EventDelay(stopUpdate?.Departure, scheduledDeparture) ?? prediction.ArrivalDelay;
        prediction.ArrivalTime = EventTime(stopUpdate?.Arrival) ?? Shift(scheduledArrival, prediction.ArrivalDelay);
        prediction.DepartureTime = EventTime(stopUpdate?.Departure) ?? Shift(scheduledDeparture, prediction.DepartureDelay);
        return prediction;
    }

    // Duragin son durumunu kopyaya yazar; planlama durumu degistiyse (iptal, silinme, atlama ya da bunlarin geri alinmasi)
    // satira yazilacak degisiklik eklenir. Yalniz gecikme degisirse satir arac ayrilana kadar beklenir.
    private void UpdateStatus(RealtimeOrganizationSnapshotModel snapshot, RealtimeTripInstanceModel instance,
        RealtimeStopTimeSnapshotModel stop, int tripCode, RealtimeStopTimePredictionModel? prediction, string correlationId,
        List<RealtimeStopTimeChangeModel> changes)
    {
        var status = new RealtimeStopStatusModel
        {
            ServiceDate = instance.ServiceDate,
            TripScheduleRelationshipCode = tripCode,
            StopScheduleRelationshipCode = IsTripRemoved(tripCode)
                ? StopScheduleRelationshipConsts.SkippedCode
                : prediction?.StopScheduleRelationshipCode ?? StopScheduleRelationshipConsts.ScheduledCode,
            ReportedDelay = prediction?.DepartureDelay,
            CorrelationId = correlationId
        };
        var key = snapshotManager.GetStopKey(instance, stop.StopSequence);
        var previous = snapshot.StopStatuses.GetValueOrDefault(key);
        snapshot.StopStatuses[key] = status;
        if (HasStatusChanged(previous, status))
            changes.Add(new RealtimeStopTimeChangeModel
            {
                Instance = instance, Stop = stop, Kind = RealtimeStopTimeChangeKind.Status, CorrelationId = correlationId,
                ViolationTypeCode = StatusViolation(status)
            });
    }

    // Planlanan kalkistan toleranstan erken ya da gec kalkan duraga ihlal kodu yazar; planlanan saati olmayan durak
    // degerlendirilmez.
    private async Task MarkDepartureViolationsAsync(List<RealtimeStopTimeChangeModel> changes,
        CancellationToken cancellationToken = default)
    {
        var early = await settingProvider.GetAsync<int>(
            GtfsRealtimeManagementSettings.Processing.EarlyDepartureToleranceSeconds);
        var late = await settingProvider.GetAsync<int>(
            GtfsRealtimeManagementSettings.Processing.LateDepartureToleranceSeconds);
        foreach (var change in changes.Where(change => change.Kind == RealtimeStopTimeChangeKind.Departure))
        {
            var delay = (change.MeasuredAt - snapshotManager.GetScheduledTime(change.Instance,
                change.Stop.DepartureTime ?? change.Stop.ArrivalTime))?.TotalSeconds;
            change.ViolationTypeCode = delay < -early ? ViolationTypeConsts.EarlyDepartureCode
                : delay > late ? ViolationTypeConsts.LateDepartureCode
                : null;
        }
    }

    // Degisiklikleri servis gunune gore gruplayip tek is biriminde mevcut satirlara uygular, eksik satirlari kurar.
    private async Task PersistAsync(Guid realtimeSourceId, RealtimeOrganizationSnapshotModel snapshot,
        List<RealtimeStopTimeChangeModel> changes, CancellationToken cancellationToken = default)
    {
        if (changes.Count == 0)
            return;
        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var lookups = await GetLookupIdsAsync(cancellationToken);
        foreach (var day in changes.GroupBy(change => change.Instance.ServiceDate))
            await PersistDayAsync(realtimeSourceId, snapshot, day.Key, day.ToList(), lookups, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    // Gunun etkilenen seferlerinin satirlarini tek sorguda okur; her degisikligi satirina uygular. Okunan satirlar is
    // biriminde izlendigi icin guncellemeler is birimi tamamlaninca kaydedilir.
    private async Task PersistDayAsync(Guid realtimeSourceId, RealtimeOrganizationSnapshotModel snapshot,
        DateOnly serviceDate, List<RealtimeStopTimeChangeModel> changes, RealtimeStopTimeLookupIdsModel lookups,
        CancellationToken cancellationToken = default)
    {
        var tripIds = changes.Select(change => change.Instance.Trip.GtfsTripId).Distinct().ToList();
        var rows = (await repository.GetListForTripsAsync(realtimeSourceId, serviceDate, tripIds, cancellationToken))
            .ToDictionary(row => RowKey(row.GtfsTripId, row.StartTime, row.StopSequence));
        var created = new List<RealtimeStopTime>();
        foreach (var change in changes)
        {
            var key = RowKey(change.Instance.Trip.GtfsTripId, change.Instance.StartTime, change.Stop.StopSequence);
            if (!rows.TryGetValue(key, out var row))
            {
                rows[key] = row = Create(realtimeSourceId, snapshot, change, lookups);
                created.Add(row);
            }
            Apply(row, snapshot, change, lookups);
        }
        await repository.InsertManyAsync(created, cancellationToken: cancellationToken);
    }

    // Sefer orneginin duragi icin satir kurar; planlama durumu biliniyorsa onunla, bilinmiyorsa planli olarak baslar.
    private RealtimeStopTime Create(Guid realtimeSourceId, RealtimeOrganizationSnapshotModel snapshot,
        RealtimeStopTimeChangeModel change, RealtimeStopTimeLookupIdsModel lookups)
    {
        var status = FindStatus(snapshot, change);
        return stopTimeManager.CreateProcessed(realtimeSourceId, snapshot.Plan!.FeedId, change.Instance, change.Stop,
            LookupId(lookups.TripScheduleRelationships, status?.TripScheduleRelationshipCode,
                TripScheduleRelationshipConsts.ScheduledCode)!.Value,
            LookupId(lookups.StopScheduleRelationships, status?.StopScheduleRelationshipCode,
                StopScheduleRelationshipConsts.ScheduledCode)!.Value);
    }

    // Degisikligin turune gore satirin giris, cikis ya da durum alanlarini Manager uzerinden yazar.
    private void Apply(RealtimeStopTime row, RealtimeOrganizationSnapshotModel snapshot, RealtimeStopTimeChangeModel change,
        RealtimeStopTimeLookupIdsModel lookups)
    {
        var status = FindStatus(snapshot, change);
        Guid? vehicleId = change.GtfsVehicleId is { } gtfsVehicleId
            && snapshot.VehicleIds.TryGetValue(gtfsVehicleId, out var id) ? id : null;
        if (change.Kind == RealtimeStopTimeChangeKind.Arrival)
            stopTimeManager.ApplyArrival(row, change.MeasuredAt!.Value, change.Latitude!.Value, change.Longitude!.Value,
                vehicleId, LookupId(lookups.VehicleStopStatuses, change.VehicleStopStatusCode, null), change.CorrelationId);
        else if (change.Kind == RealtimeStopTimeChangeKind.Status)
            stopTimeManager.ApplyStatus(row,
                LookupId(lookups.TripScheduleRelationships, status?.TripScheduleRelationshipCode,
                    TripScheduleRelationshipConsts.ScheduledCode)!.Value,
                LookupId(lookups.StopScheduleRelationships, status?.StopScheduleRelationshipCode,
                    StopScheduleRelationshipConsts.ScheduledCode)!.Value,
                LookupId(lookups.ViolationTypes, change.ViolationTypeCode, null), change.CorrelationId);
        else
            stopTimeManager.ApplyDeparture(row,
                change.Kind == RealtimeStopTimeChangeKind.Departure ? change.MeasuredAt : null,
                vehicleId, status?.ReportedDelay, LookupId(lookups.ViolationTypes, change.ViolationTypeCode, null),
                change.CorrelationId);
    }

    // GTFS-RT durum ve ihlal kodlarinin lookup kimliklerini okur.
    private async Task<RealtimeStopTimeLookupIdsModel> GetLookupIdsAsync(CancellationToken cancellationToken = default)
        => new()
        {
            TripScheduleRelationships = (await tripRelationshipRepository.GetListAsync(cancellationToken: cancellationToken))
                .ToDictionary(value => value.Code, value => value.Id),
            StopScheduleRelationships = (await stopRelationshipRepository.GetListAsync(cancellationToken: cancellationToken))
                .ToDictionary(value => value.Code, value => value.Id),
            VehicleStopStatuses = (await vehicleStopStatusRepository.GetListAsync(cancellationToken: cancellationToken))
                .ToDictionary(value => value.Code, value => value.Id),
            ViolationTypes = (await violationTypeRepository.GetListAsync(cancellationToken: cancellationToken))
                .ToDictionary(value => value.Code, value => value.Id)
        };

    // Aracin ziyaretini getirir; arac baska bir sefer ornegine gectiyse ziyaret sifirdan baslar.
    private static RealtimeVehicleVisitModel GetVisit(RealtimeOrganizationSnapshotModel snapshot, string gtfsVehicleId,
        string tripKey)
    {
        var visit = snapshot.VehicleVisits.GetOrAdd(gtfsVehicleId, _ => new RealtimeVehicleVisitModel { TripKey = tripKey });
        return visit.TripKey == tripKey
            ? visit
            : snapshot.VehicleVisits[gtfsVehicleId] = new RealtimeVehicleVisitModel { TripKey = tripKey };
    }

    // Aracin gectigi son duraktan sonraki birkac durak icinde yaricapa giren en yakin duragi bulur.
    private static RealtimeStopTimeSnapshotModel? FindStopInRadius(RealtimeTripSnapshotModel trip, int passedSequence,
        PositionModel point, double radius)
        => trip.StopTimes
            .Where(stop => stop.StopSequence > passedSequence && stop.Latitude is not null && stop.Longitude is not null)
            .Take(LookAheadStops)
            .Select(stop => (Stop: stop,
                Distance: Distance(stop.Latitude!.Value, stop.Longitude!.Value, point.Latitude, point.Longitude)))
            .Where(candidate => candidate.Distance <= radius)
            .OrderBy(candidate => candidate.Distance)
            .Select(candidate => candidate.Stop)
            .FirstOrDefault();

    // Akis hiz gondermediginde hizi bir onceki konumdan hesaplar; onceki konum yoksa bilinmez.
    private static double? DerivedSpeed(RealtimeVehicleVisitModel visit, PositionModel point, DateTime measuredAt)
        => visit.LastMeasuredAt is { } last && measuredAt > last
            ? Distance(visit.LastLatitude!.Value, visit.LastLongitude!.Value, point.Latitude, point.Longitude)
              / (measuredAt - last).TotalSeconds
            : null;

    // Iki WGS-84 noktasi arasindaki buyuk daire uzakligi, metre (haversine).
    private static double Distance(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        var dLatitude = (latitude2 - latitude1) * Math.PI / 180;
        var dLongitude = (longitude2 - longitude1) * Math.PI / 180;
        var a = Math.Pow(Math.Sin(dLatitude / 2), 2) + Math.Cos(latitude1 * Math.PI / 180)
            * Math.Cos(latitude2 * Math.PI / 180) * Math.Pow(Math.Sin(dLongitude / 2), 2);
        return 2 * EarthRadiusMeters * Math.Asin(Math.Sqrt(a));
    }

    // Iki gunden eski servis gunlerinin durumlarini kopyadan atar.
    private static void RemoveExpiredStatuses(RealtimeOrganizationSnapshotModel snapshot, DateTime measuredAt)
    {
        var oldest = DateOnly.FromDateTime(measuredAt).AddDays(-2);
        foreach (var pair in snapshot.StopStatuses.Where(pair => pair.Value.ServiceDate < oldest))
            snapshot.StopStatuses.TryRemove(pair);
    }

    private RealtimeStopStatusModel? FindStatus(RealtimeOrganizationSnapshotModel snapshot,
        RealtimeStopTimeChangeModel change)
        => snapshot.StopStatuses.GetValueOrDefault(snapshotManager.GetStopKey(change.Instance, change.Stop.StopSequence));

    private static RealtimeStopTimeChangeModel With(RealtimeStopTimeChangeModel template, RealtimeStopTimeSnapshotModel stop,
        RealtimeStopTimeChangeKind kind)
        => new()
        {
            Instance = template.Instance, Stop = stop, Kind = kind, CorrelationId = template.CorrelationId,
            GtfsVehicleId = template.GtfsVehicleId, MeasuredAt = template.MeasuredAt, Latitude = template.Latitude,
            Longitude = template.Longitude, VehicleStopStatusCode = template.VehicleStopStatusCode
        };

    // Durak bilgisi stop_sequence ile, o yoksa stop_id ile eslesir (GTFS-RT StopTimeUpdate kurali).
    private static StopTimeUpdateModel? FindStopUpdate(TripUpdateModel update, RealtimeStopTimeSnapshotModel stop)
        => update.StopTimeUpdate.FirstOrDefault(candidate =>
            candidate.StopSequence == (uint)stop.StopSequence
            || (candidate.StopSequence is null && candidate.StopId == stop.GtfsStopId));

    // Bildirilen mutlak zaman varsa gecikme ondan hesaplanir; yoksa bildirilen gecikme kullanilir.
    private static int? EventDelay(StopTimeEventModel? stopTimeEvent, DateTime? scheduled)
        => stopTimeEvent?.Time is { } time && scheduled is { } at
            ? (int)(ToUtc(time) - at).TotalSeconds
            : stopTimeEvent?.Delay;

    private static DateTime? EventTime(StopTimeEventModel? stopTimeEvent)
        => stopTimeEvent?.Time is { } time ? ToUtc(time) : null;

    private static DateTime? Shift(DateTime? scheduled, int? delay)
        => scheduled is { } at && delay is { } seconds ? at.AddSeconds(seconds) : null;

    private static DateTime ToUtc(long unixSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;

    // Iptal ya da silinen seferin duragi iptal ihlali, atlanan durak durak atlama ihlali tasir.
    private static int? StatusViolation(RealtimeStopStatusModel status)
        => IsTripRemoved(status.TripScheduleRelationshipCode) ? ViolationTypeConsts.CanceledTripCode
            : status.StopScheduleRelationshipCode == StopScheduleRelationshipConsts.SkippedCode
                ? ViolationTypeConsts.SkippedStopCode
                : null;

    private static bool IsTripRemoved(int tripCode)
        => tripCode is TripScheduleRelationshipConsts.CanceledCode or TripScheduleRelationshipConsts.DeletedCode;

    // Planli olmayan bir durum ilk kez geldiyse ya da onceki durumdan farkliysa satira yazilir.
    private static bool HasStatusChanged(RealtimeStopStatusModel? previous, RealtimeStopStatusModel current)
    {
        var isDefault = current.TripScheduleRelationshipCode == TripScheduleRelationshipConsts.ScheduledCode
            && current.StopScheduleRelationshipCode == StopScheduleRelationshipConsts.ScheduledCode;
        return previous is null
            ? !isDefault
            : previous.TripScheduleRelationshipCode != current.TripScheduleRelationshipCode
              || previous.StopScheduleRelationshipCode != current.StopScheduleRelationshipCode;
    }

    private static int? LookupId(Dictionary<int, int> lookup, int? code, int? fallbackCode)
        => code is { } value && lookup.TryGetValue(value, out var id) ? id
            : fallbackCode is { } fallback && lookup.TryGetValue(fallback, out var fallbackId) ? fallbackId : null;

    private static string RowKey(string gtfsTripId, string? startTime, int stopSequence)
        => $"{gtfsTripId}|{startTime}|{stopSequence}";
}
