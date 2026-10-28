using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Interfaces;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSources;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeStopTimes;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace Byd.GtfsRealtimeManagement.Managers.RealtimeStopTimes;

// islevi: Durak satirinin aktif kaynak, lookup ve tekillik kurallarini uygular.
// sistemdeki gorevi: CRUD mutasyonlarinin domain sahibidir.
public class RealtimeStopTimeManager(
    IRealtimeStopTimeRepository repository,
    RealtimeSourceManager sourceManager,
    IRepository<TripScheduleRelationship, int> tripRelationshipRepository,
    IRepository<StopScheduleRelationship, int> stopRelationshipRepository,
    IRepository<VehicleStopStatus, int> vehicleStatusRepository,
    IRepository<ViolationType, int> violationTypeRepository)
    : BaseManager<RealtimeStopTime, Guid>(repository)
{
    private IDataFilter DataFilter => LazyServiceProvider.LazyGetRequiredService<IDataFilter>();

    // Organizasyonun aktif kaynagi altinda yeni satir kurar.
    public virtual async Task<RealtimeStopTime> CreateAsync(
        RealtimeStopTimeCreateModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var source = await sourceManager.GetActiveInScopeAsync(cancellationToken);
        var entity = new RealtimeStopTime(GuidGenerator.Create(), source.Id) { RealtimeSource = source };
        await ApplyAsync(entity, model, cancellationToken);
        return entity;
    }

    // Aktif kaynak satirini gunceller; kaynak kimligi degismez.
    public virtual Task UpdateAsync(
        RealtimeStopTime entity, RealtimeStopTimeUpdateModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(model);
        return ApplyAsync(entity, model, cancellationToken);
    }

    // Id'yi yalnizca istegin aktif kaynaginda arar ve lookup adlarini getirir.
    public virtual async Task<RealtimeStopTime> EnsureExistsInScopeAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        await sourceManager.GetActiveInScopeAsync(cancellationToken);
        return await repository.FindWithLookupsAsync(id, cancellationToken)
            ?? throw new BusinessException(GeneralExceptionCodes.EntityNotFound);
    }

    // Belirli kaynagin pasif ya da gecmis satirini getirir; kaynak istegin kapsaminda olmali.
    public virtual async Task<RealtimeStopTime> EnsureExistsForSourceAsync(
        Guid realtimeSourceId, Guid id, CancellationToken cancellationToken = default)
    {
        using var inactiveIncluded = DataFilter.Disable<ActiveRealtimeSourceFilter>();
        await sourceManager.EnsureExistsInScopeAsync(realtimeSourceId, cancellationToken);
        var entity = await repository.FindWithLookupsAsync(id, cancellationToken);
        return entity is not null && entity.RealtimeSourceId == realtimeSourceId
            ? entity
            : throw new BusinessException(GeneralExceptionCodes.EntityNotFound);
    }

    // Liste filtresindeki kaynak alanini, istegin kapsaminda dogrulanan kaynaga kilitler.
    public virtual async Task ScopeFilterAsync(
        IRealtimeSourceFilter filter, Guid realtimeSourceId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        using var inactiveIncluded = DataFilter.Disable<ActiveRealtimeSourceFilter>();
        await sourceManager.EnsureExistsInScopeAsync(realtimeSourceId, cancellationToken);
        filter.RealtimeSourceId = new DataTableFilterItem
        {
            Operator = DataTableFilterOperator.Equal,
            Value = realtimeSourceId.ToString()
        };
    }

    // Isleme adiminin ilk kez gordugu sefer orneginin duragi icin satir kurar; durum kimlikleri lookup tablosundan gelir.
    public virtual RealtimeStopTime CreateProcessed(Guid realtimeSourceId, Guid gtfsFeedId,
        RealtimeTripInstanceModel instance, RealtimeStopTimeSnapshotModel stop,
        int tripScheduleRelationshipId, int stopScheduleRelationshipId)
        => new(GuidGenerator.Create(), realtimeSourceId)
        {
            GtfsFeedId = gtfsFeedId,
            ServiceDate = instance.ServiceDate,
            StartTime = instance.StartTime,
            GtfsTripId = instance.Trip.GtfsTripId,
            GtfsRouteId = instance.Trip.GtfsRouteId,
            StopSequence = stop.StopSequence,
            GtfsStopId = stop.GtfsStopId ?? string.Empty,
            TripScheduleRelationshipId = tripScheduleRelationshipId,
            StopScheduleRelationshipId = stopScheduleRelationshipId
        };

    // Aracin duraga girisini, konumunu ve akisin bildirdigi durak durumunu kanit correlation'iyla yazar.
    public virtual void ApplyArrival(RealtimeStopTime entity, DateTime arrivalTime, double latitude, double longitude,
        Guid? vehicleId, int? vehicleStopStatusId, string correlationId)
    {
        entity.ArrivalTime = arrivalTime;
        entity.Latitude = latitude;
        entity.Longitude = longitude;
        entity.VehicleId = vehicleId ?? entity.VehicleId;
        entity.VehicleStopStatusId = vehicleStopStatusId;
        entity.ArrivalCorrelationId = correlationId;
    }

    // Aracin duraktan cikisini ya da duragi gectigini yazar; durmadan gecilen durakta cikis zamani bos kalir. Kaynagin
    // arac ayrilmadan once bildirdigi son gecikme de bu anda yazilir.
    public virtual void ApplyDeparture(RealtimeStopTime entity, DateTime? departureTime, Guid? vehicleId,
        int? reportedDelay, int? violationTypeId, string correlationId)
    {
        entity.DepartureTime = departureTime;
        entity.VehicleId = vehicleId ?? entity.VehicleId;
        entity.ReportedDelay = reportedDelay ?? entity.ReportedDelay;
        entity.ViolationTypeId = violationTypeId ?? entity.ViolationTypeId;
        entity.DepartureCorrelationId = correlationId;
    }

    // Sefer ve durak planlama durumunu kanit correlation'iyla yazar.
    public virtual void ApplyStatus(RealtimeStopTime entity, int tripScheduleRelationshipId,
        int stopScheduleRelationshipId, int? violationTypeId, string correlationId)
    {
        entity.TripScheduleRelationshipId = tripScheduleRelationshipId;
        entity.StopScheduleRelationshipId = stopScheduleRelationshipId;
        entity.ViolationTypeId = violationTypeId ?? entity.ViolationTypeId;
        entity.StatusCorrelationId = correlationId;
    }

    // Girdi alanlarini uygular ve sefer ornegi/durak sirasini tekillestirir.
    private async Task ApplyAsync(
        RealtimeStopTime entity, RealtimeStopTimeInputModel model, CancellationToken cancellationToken = default)
    {
        await ResolveLookupsAsync(entity, model, cancellationToken);
        ApplyIdentity(entity, model);
        ApplyMeasurements(entity, model);
        await EnsureUniqueAsync(entity, cancellationToken);
    }

    // Lookup baglarini varlik denetimiyle kurar.
    private async Task ResolveLookupsAsync(
        RealtimeStopTime entity, RealtimeStopTimeInputModel model, CancellationToken cancellationToken = default)
    {
        entity.TripScheduleRelationship = await tripRelationshipRepository.GetAsync(
            model.TripScheduleRelationshipId, cancellationToken: cancellationToken);
        entity.TripScheduleRelationshipId = entity.TripScheduleRelationship.Id;
        entity.StopScheduleRelationship = await stopRelationshipRepository.GetAsync(
            model.StopScheduleRelationshipId, cancellationToken: cancellationToken);
        entity.StopScheduleRelationshipId = entity.StopScheduleRelationship.Id;
        entity.VehicleStopStatus = model.VehicleStopStatusId is { } statusId
            ? await vehicleStatusRepository.GetAsync(statusId, cancellationToken: cancellationToken) : null;
        entity.VehicleStopStatusId = entity.VehicleStopStatus?.Id;
        entity.ViolationType = model.ViolationTypeId is { } violationId
            ? await violationTypeRepository.GetAsync(violationId, cancellationToken: cancellationToken) : null;
        entity.ViolationTypeId = entity.ViolationType?.Id;
    }

    // Kaynak altindaki sefer/durak anahtarini ve harici kimlikleri uygular.
    private static void ApplyIdentity(RealtimeStopTime entity, RealtimeStopTimeInputModel model)
    {
        entity.GtfsFeedId = model.GtfsFeedId;
        entity.VehicleId = model.VehicleId;
        entity.ServiceDate = model.ServiceDate;
        entity.StartTime = NormalizeOptionalText(model.StartTime);
        entity.GtfsTripId = NormalizeRequiredText(model.GtfsTripId);
        entity.GtfsRouteId = NormalizeRequiredText(model.GtfsRouteId);
        entity.StopSequence = model.StopSequence;
        entity.GtfsStopId = NormalizeRequiredText(model.GtfsStopId);
    }

    // Olcumleri ve ham log kanit kimliklerini uygular.
    private static void ApplyMeasurements(RealtimeStopTime entity, RealtimeStopTimeInputModel model)
    {
        entity.ArrivalTime = model.ArrivalTime;
        entity.DepartureTime = model.DepartureTime;
        entity.Latitude = model.Latitude;
        entity.Longitude = model.Longitude;
        entity.ReportedDelay = model.ReportedDelay;
        entity.ArrivalCorrelationId = NormalizeOptionalText(model.ArrivalCorrelationId);
        entity.DepartureCorrelationId = NormalizeOptionalText(model.DepartureCorrelationId);
        entity.StatusCorrelationId = NormalizeOptionalText(model.StatusCorrelationId);
    }

    // Ayni kaynak, gun, sefer ornegi ve durak sirasinda ikinci satiri engeller.
    private Task EnsureUniqueAsync(RealtimeStopTime entity, CancellationToken cancellationToken)
    {
        var sourceId = entity.RealtimeSourceId;
        var serviceDate = entity.ServiceDate;
        var tripId = entity.GtfsTripId;
        var startTime = entity.StartTime;
        var sequence = entity.StopSequence;
        return CheckAlreadyExistsAsync(value => value.RealtimeSourceId == sourceId && value.ServiceDate == serviceDate
            && value.GtfsTripId == tripId && value.StartTime == startTime && value.StopSequence == sequence,
            entity.Id, cancellationToken);
    }
}
