using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Byd.GtfsRealtimeManagement.Localization;
using Byd.GtfsRealtimeManagement.Permissions;
using Byd.GtfsRealtimeManagement.Services.RealtimeSources;
using Byd.LogManagement.ActivityLogs;
using Byd.LogManagement.Attributes;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Byd.GtfsRealtimeManagement.Controllers.RealtimeSources;

// islevi: Kaynak CRUD islemlerinin HTTP ve yetki metadatasini sunar.
// sistemdeki gorevi: RealtimeSource use case'lerini AppService'e yonlendirir.
[ApiController]
[Route(RealtimeSourceRoutes.Root)]
public class RealtimeSourceController : GtfsRealtimeManagementController
{
    private IRealtimeSourceAppService AppService => LazyGetRequiredService<IRealtimeSourceAppService>();

    /// <summary>Istegin organizasyonuna yeni realtime kaynagi ekler.</summary>
    /// <param name="input">Kaynak adi ve adresleri.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { Name (string) → organizasyonda tekil ad; VehiclePositionsUrl, TripUpdatesUrl,
    /// ServiceAlertsUrl (string?) → en az biri dolu mutlak HTTP(S) adresleri }
    /// Response { RealtimeSourceDto → kimlik, adresler, etkinlik ve audit zamanlari }
    /// Tenant ve organizasyon istek baglamindan gelir; yeni kaynak pasiftir. Bu endpoint akis cekmez.
    /// </remarks>
    [HttpPost]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.Manage)]
    public virtual async Task<Result<RealtimeSourceDto>> CreateAsync(
        [FromBody] CreateRealtimeSourceDto input, CancellationToken cancellationToken = default)
        => await AppService.CreateAsync(input, cancellationToken);

    /// <summary>Kimligi verilen realtime kaynagini getirir.</summary>
    /// <param name="id">Kaynak kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { RealtimeSourceDto → kimlik, adresler, etkinlik ve audit zamanlari }
    /// Yalnizca istegin organizasyonundaki kaynak gorulur. Bu endpoint akis cekmez.
    /// </remarks>
    [HttpGet(RealtimeSourceRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.View)]
    public virtual async Task<Result<RealtimeSourceDto>> GetAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await AppService.GetAsync(id, cancellationToken);

    /// <summary>Pasif kaynak dahil kimligi verilen realtime kaynagini getirir.</summary>
    /// <param name="id">Kaynak kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { RealtimeSourceDto → kimlik, adresler, etkinlik ve audit zamanlari }
    /// Yalnizca istegin organizasyonundaki kaynak gorulur. Bu endpoint akis cekmez.
    /// </remarks>
    [HttpGet(RealtimeSourceRoutes.IncludingInactive)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.Manage)]
    public virtual async Task<Result<RealtimeSourceDto>> GetIncludingInactiveAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await AppService.GetIncludingInactiveAsync(id, cancellationToken);

    /// <summary>Filtreli ve sayfalanmis realtime kaynaklarini getirir.</summary>
    /// <param name="input">Ad, etkinlik, kayit zamani ve sayfalama filtreleri.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { Page (int) → sayfa; PageSize (int) → boyut; Sorting (DataTableSorting[]?) → siralama;
    /// Filters (RealtimeSourceFilter?) → Name, IsActive, CreationTime alan filtreleri }
    /// Response { TotalCount (long) → filtreye uyan sayi; Items (RealtimeSourceDto[]) → sayfa }
    /// Etkin olmayan kaynaklar da gorulur. Bu endpoint akis cekmez.
    /// </remarks>
    [HttpGet(RealtimeSourceRoutes.FilteredList)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.Manage)]
    public virtual async Task<Result<PagedResultDto<RealtimeSourceDto>>> GetFilteredListAsync(
        [FromQuery] DataTableListInput<RealtimeSourceFilter> input, CancellationToken cancellationToken = default)
        => await AppService.GetFilteredListAsync(input, cancellationToken);

    /// <summary>Kimligi verilen realtime kaynagini gunceller.</summary>
    /// <param name="id">Guncellenecek kaynak kimligi.</param>
    /// <param name="input">Yeni ad ve adresler.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { Name (string) → organizasyonda tekil ad; VehiclePositionsUrl, TripUpdatesUrl,
    /// ServiceAlertsUrl (string?) → en az biri dolu mutlak HTTP(S) adresleri }
    /// Response { RealtimeSourceDto → guncel alanlar ve audit zamanlari }
    /// Tenant ve organizasyon degismez; islem activity log'a yazilir. Bu endpoint akis cekmez.
    /// </remarks>
    [HttpPut(RealtimeSourceRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.Manage)]
    [TrackActivity<GtfsRealtimeManagementResource>(
        GtfsRealtimeManagementLocalizationKeys.ActivityLogs.RealtimeSourceUpdated,
        ActivityKind.Updated, nameof(RealtimeSourceDto.Id))]
    public virtual async Task<Result<RealtimeSourceDto>> UpdateAsync(
        Guid id, [FromBody] UpdateRealtimeSourceDto input, CancellationToken cancellationToken = default)
        => await AppService.UpdateAsync(id, input, cancellationToken);

    /// <summary>Kaynagi organizasyonun aktif kaynagi yapar.</summary>
    /// <param name="id">Etkinlestirilecek kaynak kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { RealtimeSourceDto → etkin kaynak ve adresleri }
    /// Onceki aktif kaynak pasiflesir. Bu endpoint akis cekmez.
    /// </remarks>
    [HttpPost(RealtimeSourceRoutes.Activate)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.Manage)]
    [TrackActivity<GtfsRealtimeManagementResource>(
        GtfsRealtimeManagementLocalizationKeys.ActivityLogs.RealtimeSourceActivated,
        ActivityKind.Updated, nameof(RealtimeSourceDto.Id))]
    public virtual async Task<Result<RealtimeSourceDto>> ActivateAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await AppService.ActivateAsync(id, cancellationToken);

    /// <summary>Kimligi verilen realtime kaynagini siler.</summary>
    /// <param name="id">Silinecek kaynak kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { govde yok; silme kalicidir }
    /// Yalnizca istegin organizasyonundaki kaynak silinir ve activity log'a yazilir. Bu endpoint akis cekmez.
    /// </remarks>
    [HttpDelete(RealtimeSourceRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.Manage)]
    [TrackActivity<GtfsRealtimeManagementResource>(
        GtfsRealtimeManagementLocalizationKeys.ActivityLogs.RealtimeSourceDeleted,
        ActivityKind.Deleted, nameof(RealtimeSourceDto.Id))]
    public virtual async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AppService.DeleteAsync(id, cancellationToken);
        return Result.NoContent();
    }

    /// <summary>Organizasyonun aktif kaynaginin VehiclePositions adresindeki arac konumlarini anlik ceker.</summary>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { VehiclePositionDto[] → GTFS-RT VehiclePosition: Trip (TripId, RouteId, DirectionId, StartTime,
    ///   StartDate, ScheduleRelationship); Vehicle (Id, Label, LicensePlate, WheelchairAccessible);
    ///   Position (Latitude, Longitude, Bearing, Odometer, Speed m/s); CurrentStopSequence; StopId; CurrentStatus;
    ///   Timestamp (POSIX saniye); CongestionLevel; OccupancyStatus; OccupancyPercentage; MultiCarriageDetails[] }
    /// Gelmeyen alan bos doner; enum alanlari standarttaki sayi kodudur. Cagri ve ham yanit log modulune yazilir.
    /// Aktif kaynakta VehiclePositions adresi yoksa hata verir. Bu endpoint veriyi kaydetmez.
    /// </remarks>
    [HttpGet(RealtimeSourceRoutes.VehiclePositions)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.View)]
    public virtual async Task<Result<List<VehiclePositionDto>>> GetVehiclePositionsAsync(
        CancellationToken cancellationToken = default)
        => await AppService.GetVehiclePositionsAsync(cancellationToken);

    /// <summary>Organizasyonun aktif kaynaginin TripUpdates adresindeki sefer guncellemelerini anlik ceker.</summary>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { TripUpdateDto[] → GTFS-RT TripUpdate: Trip (TripId, RouteId, DirectionId, StartTime, StartDate,
    ///   ScheduleRelationship); Vehicle (Id, Label, LicensePlate); StopTimeUpdate[] (StopSequence, StopId,
    ///   Arrival/Departure { Delay sn, Time, Uncertainty, ScheduledTime }, ScheduleRelationship, StopTimeProperties);
    ///   Timestamp (POSIX saniye); Delay sn; TripProperties }
    /// Gelmeyen alan bos doner; enum alanlari standarttaki sayi kodudur. Cagri ve ham yanit log modulune yazilir.
    /// Aktif kaynakta TripUpdates adresi yoksa hata verir. Bu endpoint veriyi kaydetmez.
    /// </remarks>
    [HttpGet(RealtimeSourceRoutes.TripUpdates)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.View)]
    public virtual async Task<Result<List<TripUpdateDto>>> GetTripUpdatesAsync(
        CancellationToken cancellationToken = default)
        => await AppService.GetTripUpdatesAsync(cancellationToken);

    /// <summary>Organizasyonun aktif kaynaginin ServiceAlerts adresindeki uyarilari anlik ceker.</summary>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { AlertDto[] → GTFS-RT Alert: ActivePeriod/CommunicationPeriod/ImpactPeriod[] (Start, End);
    ///   InformedEntity[] (AgencyId, RouteId, RouteType, Trip, StopId, DirectionId); Cause; Effect; Url;
    ///   HeaderText/DescriptionText (Translation[] { Text, Language }); SeverityLevel; Image; CauseDetail; EffectDetail }
    /// Gelmeyen alan bos doner; enum alanlari standarttaki sayi kodudur. Cagri ve ham yanit log modulune yazilir.
    /// Aktif kaynakta ServiceAlerts adresi yoksa hata verir. Bu endpoint veriyi kaydetmez.
    /// </remarks>
    [HttpGet(RealtimeSourceRoutes.ServiceAlerts)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.View)]
    public virtual async Task<Result<List<AlertDto>>> GetServiceAlertsAsync(
        CancellationToken cancellationToken = default)
        => await AppService.GetServiceAlertsAsync(cancellationToken);

    /// <summary>Organizasyonun aktif kaynaginin butun akislarini tek GTFS-RT FeedMessage olarak anlik ceker.</summary>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { FeedMessageDto → Header (GtfsRealtimeVersion en yeni surum, Incrementality, Timestamp en eski akisin
    ///   POSIX saniyesi, FeedVersion); Entity[] → Id, IsDeleted ve akisina gore Vehicle, TripUpdate ya da Alert }
    /// Aktif kaynakta adresi tanimli her akis cekilir; tanimsiz akis atlanir. Her cagri ve ham yanit log modulune
    /// ayri yazilir. Gelmeyen alan bos doner; enum alanlari standarttaki sayi kodudur. Bu endpoint veriyi kaydetmez.
    /// </remarks>
    [HttpGet(RealtimeSourceRoutes.FeedMessage)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.View)]
    public virtual async Task<Result<FeedMessageDto>> GetFeedMessageAsync(
        CancellationToken cancellationToken = default)
        => await AppService.GetFeedMessageAsync(cancellationToken);
}
