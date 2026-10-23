using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Managers;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Interfaces;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Managers.RealtimeSources;
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

    // Pasif kaynak gecmisini de tenant ve organizasyon kapsaminda okur.
    public virtual async Task<RealtimeStopTime> EnsureExistsIncludingInactiveInScopeAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        using var inactiveIncluded = DataFilter.Disable<ActiveRealtimeSourceFilter>();
        return await repository.FindWithLookupsAsync(id, cancellationToken)
            ?? throw new BusinessException(GeneralExceptionCodes.EntityNotFound);
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
