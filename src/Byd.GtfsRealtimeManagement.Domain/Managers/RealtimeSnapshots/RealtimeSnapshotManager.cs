using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Etos.Vehicles;
using Byd.GtfsRealtimeManagement.ExceptionCodes;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Byd.GtfsRealtimeManagement.Stores.RealtimeSnapshots;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;

namespace Byd.GtfsRealtimeManagement.Managers.RealtimeSnapshots;

// islevi: Organizasyonun arac ve aktif GTFS plan kopyasini yukler, arac ve feed olaylariyla gunceller ve akista gorulen
// kayitsiz araclar icin arac modulunden kayit ister.
// sistemdeki gorevi: Realtime'in arac ve plan verisini her akista veritabanina gitmeden okumasini saglar. Kopya
// organizasyonun ilk cekmesinde yuklenir; sonrasinda yalniz olaylarla degisir, periyodik sorgu atilmaz.
public class RealtimeSnapshotManager(
    IRealtimeSnapshotRepository repository,
    RealtimeSnapshotStore store,
    IDistributedEventBus distributedEventBus,
    ICurrentOrganization currentOrganization) : DomainService
{
    // Istegin organizasyonunun kopyasi yoksa araclarini ve aktif planini yukler.
    public virtual async Task<RealtimeOrganizationSnapshotModel> EnsureLoadedAsync(
        CancellationToken cancellationToken = default)
    {
        if (CurrentTenant.Id is not { } tenantId || currentOrganization.Id is not { } organizationId)
            throw new BusinessException(RealtimeSourceExceptionCodes.ScopeRequired);
        return store.Find(organizationId) ?? store.Add(organizationId,
            await LoadAsync(tenantId, organizationId, cancellationToken));
    }

    // Akistaki arac konumlarindan kopyada olmayan ve daha once istenmemis her arac icin tek kayit olayi yayinlar.
    public virtual async Task RequestUnregisteredVehiclesAsync(
        FeedMessageModel message, CancellationToken cancellationToken = default)
    {
        var snapshot = await EnsureLoadedAsync(cancellationToken);
        foreach (var position in message.Entity.Select(entity => entity.Vehicle).OfType<VehiclePositionModel>())
        {
            if (position.Vehicle?.Id is not { Length: > 0 } gtfsVehicleId
                || snapshot.VehicleIds.ContainsKey(gtfsVehicleId)
                || !snapshot.RequestedVehicleIds.TryAdd(gtfsVehicleId, 0))
                continue;
            await distributedEventBus.PublishAsync(ToRegistrationRequest(snapshot, position, gtfsVehicleId));
        }
    }

    // Eklenen ya da guncellenen aracin kimligini kopyaya yazar; kopya henuz yuklenmediyse arac ilk yuklemede gelir.
    public virtual void SetVehicle(VehicleEto vehicle)
    {
        if (store.Find(vehicle.OrganizationId) is not { } snapshot)
            return;
        RemoveVehicleId(snapshot, vehicle.Id);
        snapshot.VehicleIds[vehicle.GtfsVehicleId] = vehicle.Id;
        snapshot.RequestedVehicleIds.TryRemove(vehicle.GtfsVehicleId, out _);
    }

    // Silinen aracin kimligini kopyadan cikarir; arac akista yeniden gorulurse yeniden kayit istenir.
    public virtual void RemoveVehicle(VehicleEto vehicle)
    {
        if (store.Find(vehicle.OrganizationId) is { } snapshot)
            RemoveVehicleId(snapshot, vehicle.Id);
    }

    // Organizasyonun aktif plani degistiginde plani yeniden yukler; kopya henuz yuklenmediyse plan ilk yuklemede gelir.
    public virtual async Task ReloadPlanAsync(
        Guid tenantId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        if (store.Find(organizationId) is { } snapshot)
            snapshot.Plan = await LoadPlanAsync(tenantId, organizationId, cancellationToken);
    }

    // Seferin servis gununde planda calisip calismadigini GTFS takvim kuralina gore verir: o gunun calendar_dates
    // istisnasi haftalik takvimin onune gecer; sefer ya da servisi planda yoksa calismaz sayilir.
    public virtual bool IsTripScheduled(RealtimePlanSnapshotModel plan, string gtfsTripId, DateOnly serviceDate)
    {
        if (!plan.Trips.TryGetValue(gtfsTripId, out var trip)
            || !plan.Services.TryGetValue(trip.GtfsServiceId, out var service))
            return false;
        if (service.Exceptions.TryGetValue(serviceDate, out var exceptionType))
            return exceptionType == RealtimeSnapshotConsts.ServiceAddedExceptionType;
        return service.StartDate <= serviceDate && serviceDate <= service.EndDate
            && RunsOn(service, serviceDate.DayOfWeek);
    }

    // Araclari ve aktif plani okuyup organizasyon kopyasini kurar.
    private async Task<RealtimeOrganizationSnapshotModel> LoadAsync(
        Guid tenantId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var snapshot = new RealtimeOrganizationSnapshotModel
        {
            Plan = await LoadPlanAsync(tenantId, organizationId, cancellationToken)
        };
        foreach (var vehicle in await repository.GetVehiclesAsync(tenantId, organizationId, cancellationToken))
            snapshot.VehicleIds[vehicle.GtfsVehicleId] = vehicle.VehicleId;
        Logger.LogInformation("Realtime vehicle snapshot loaded: organization {OrganizationId}, {VehicleCount} vehicles.",
            organizationId, snapshot.VehicleIds.Count);
        return snapshot;
    }

    // Aktif plani okur; yukleme agir oldugu icin hangi feed'in kac seferle yuklendigi log'a yazilir.
    private async Task<RealtimePlanSnapshotModel?> LoadPlanAsync(
        Guid tenantId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var plan = await repository.GetActivePlanAsync(tenantId, organizationId, cancellationToken);
        Logger.LogInformation(
            "Realtime plan snapshot loaded: organization {OrganizationId}, feed {FeedId}, {TripCount} trips, {ServiceCount} services.",
            organizationId, plan?.FeedId, plan?.Trips.Count ?? 0, plan?.Services.Count ?? 0);
        return plan;
    }

    // Kayit istegini akistaki arac bilgisiyle kurar; kip ve isletmeci aracin seferinin plandaki hattindan gelir, sefer
    // planda yoksa bos kalir.
    private VehicleRegistrationRequestedEto ToRegistrationRequest(
        RealtimeOrganizationSnapshotModel snapshot, VehiclePositionModel position, string gtfsVehicleId)
    {
        var trip = position.Trip?.TripId is { } tripId ? snapshot.Plan?.Trips.GetValueOrDefault(tripId) : null;
        return new VehicleRegistrationRequestedEto
        {
            TenantId = CurrentTenant.Id!.Value,
            OrganizationId = currentOrganization.Id!.Value,
            GtfsVehicleId = gtfsVehicleId,
            GtfsAgencyId = trip?.GtfsAgencyId,
            Label = position.Vehicle!.Label,
            RegistrationNumber = position.Vehicle.LicensePlate,
            RouteType = trip?.RouteType,
            WheelchairAccessible = position.Vehicle.WheelchairAccessible
        };
    }

    // Haftalik takvimde servisin o haftanin gununde acik olup olmadigini verir.
    private static bool RunsOn(RealtimeServiceSnapshotModel service, DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => service.Monday,
        DayOfWeek.Tuesday => service.Tuesday,
        DayOfWeek.Wednesday => service.Wednesday,
        DayOfWeek.Thursday => service.Thursday,
        DayOfWeek.Friday => service.Friday,
        DayOfWeek.Saturday => service.Saturday,
        _ => service.Sunday
    };

    // Arac kimligi degismis olabilecegi icin eski GTFS-RT kimligine bagli kaydi temizler.
    private static void RemoveVehicleId(RealtimeOrganizationSnapshotModel snapshot, Guid vehicleId)
    {
        foreach (var pair in snapshot.VehicleIds.Where(pair => pair.Value == vehicleId))
            snapshot.VehicleIds.TryRemove(pair);
    }
}
