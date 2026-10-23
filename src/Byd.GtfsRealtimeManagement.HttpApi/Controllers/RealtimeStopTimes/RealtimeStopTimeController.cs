using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Localization;
using Byd.GtfsRealtimeManagement.Permissions;
using Byd.GtfsRealtimeManagement.Services.RealtimeStopTimes;
using Byd.LogManagement.ActivityLogs;
using Byd.LogManagement.Attributes;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Byd.GtfsRealtimeManagement.Controllers.RealtimeStopTimes;

// islevi: Durak olcumu CRUD ve gecmis okuma HTTP eylemlerini sunar.
// sistemdeki gorevi: Yetki, route ve cevap metadatasini AppService'e baglar.
[ApiController]
[Route(RealtimeStopTimeRoutes.Root)]
public class RealtimeStopTimeController : GtfsRealtimeManagementController
{
    private IRealtimeStopTimeAppService AppService => LazyGetRequiredService<IRealtimeStopTimeAppService>();

    /// <summary>Organizasyonun aktif kaynagina durak olcumu ekler.</summary>
    /// <param name="input">Sefer, durak, durum ve olcum bilgileri.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { TripScheduleRelationshipId, StopScheduleRelationshipId (int) → lookup baglari;
    /// VehicleStopStatusId, ViolationTypeId (int?) → istege bagli lookup baglari;
    /// GtfsFeedId (Guid), VehicleId (Guid?) → harici kayit kimlikleri;
    /// ServiceDate (DateOnly), StartTime (string?), GtfsTripId, GtfsRouteId (string),
    /// StopSequence (int), GtfsStopId (string) → sefer ornegi ve durak anahtari;
    /// ArrivalTime, DepartureTime (DateTime?), Latitude, Longitude (double?), ReportedDelay (int?) → olcumler;
    /// ArrivalCorrelationId, DepartureCorrelationId, StatusCorrelationId (string?) → ham kayit kanitlari }
    /// Response { RealtimeStopTimeDto → kimlik, kaynak/lookup adlari, olcumler ve audit zamanlari }
    /// Kaynak istegin tenant ve organizasyonundaki aktif kaynaktan secilir. Bu endpoint dis akis cekmez.
    /// </remarks>
    [HttpPost]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeStopTimes.Manage)]
    public virtual async Task<Result<RealtimeStopTimeDto>> CreateAsync(
        [FromBody] CreateRealtimeStopTimeDto input, CancellationToken cancellationToken = default)
        => await AppService.CreateAsync(input, cancellationToken);

    /// <summary>Aktif kaynaktaki durak olcumunu kimlikle getirir.</summary>
    /// <param name="id">Durak olcumu kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { RealtimeStopTimeDto → kaynak/lookup adlari, sefer ve durak bilgisi, olcumler, kanit kimlikleri }
    /// Yalnizca istegin tenant ve organizasyonundaki aktif kaynak okunur. Bu endpoint kayit yazmaz.
    /// </remarks>
    [HttpGet(RealtimeStopTimeRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeStopTimes.View)]
    public virtual async Task<Result<RealtimeStopTimeDto>> GetAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await AppService.GetAsync(id, cancellationToken);

    /// <summary>Pasif kaynak gecmisindeki durak olcumunu da kimlikle getirir.</summary>
    /// <param name="id">Durak olcumu kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { RealtimeStopTimeDto → kaynak/lookup adlari, sefer ve durak bilgisi, olcumler, kanit kimlikleri }
    /// Aktiflik filtresi kapatilir; tenant ve organizasyon siniri kalir. Bu endpoint kayit yazmaz.
    /// </remarks>
    [HttpGet(RealtimeStopTimeRoutes.IncludingInactive)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeStopTimes.Manage)]
    public virtual async Task<Result<RealtimeStopTimeDto>> GetIncludingInactiveAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await AppService.GetIncludingInactiveAsync(id, cancellationToken);

    /// <summary>Aktif kaynagin durak olcumlerini filtreli ve sayfali getirir.</summary>
    /// <param name="input">Filtre, siralama ve sayfalama bilgileri.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { Page (int) → sayfa; PageSize (int) → boyut; Sorting (DataTableSorting[]?) → siralama;
    /// Filters (RealtimeStopTimeFilter?) → ServiceDate, GtfsTripId, GtfsRouteId, VehicleId,
    /// ViolationTypeId alan filtreleri }
    /// Response { TotalCount (long) → filtreye uyan sayi; Items (RealtimeStopTimeDto[]) → sayfa }
    /// Yalnizca aktif kaynak gorulur; eslesme yoksa liste bostur. Bu endpoint kayit yazmaz.
    /// </remarks>
    [HttpGet(RealtimeStopTimeRoutes.FilteredList)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeStopTimes.View)]
    public virtual async Task<Result<PagedResultDto<RealtimeStopTimeDto>>> GetFilteredListAsync(
        [FromQuery] DataTableListInput<RealtimeStopTimeFilter> input, CancellationToken cancellationToken = default)
        => await AppService.GetFilteredListAsync(input, cancellationToken);

    /// <summary>Pasif kaynak gecmisi dahil durak olcumlerini filtreli getirir.</summary>
    /// <param name="input">Filtre, siralama ve sayfalama bilgileri.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { Page (int) → sayfa; PageSize (int) → boyut; Sorting (DataTableSorting[]?) → siralama;
    /// Filters (RealtimeStopTimeFilter?) → ServiceDate, GtfsTripId, GtfsRouteId, VehicleId,
    /// ViolationTypeId alan filtreleri }
    /// Response { TotalCount (long) → filtreye uyan sayi; Items (RealtimeStopTimeDto[]) → sayfa }
    /// Aktiflik filtresi kapatilir; tenant ve organizasyon siniri kalir. Eslesme yoksa liste bostur; kayit yazilmaz.
    /// </remarks>
    [HttpGet(RealtimeStopTimeRoutes.FilteredListIncludingInactive)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeStopTimes.Manage)]
    public virtual async Task<Result<PagedResultDto<RealtimeStopTimeDto>>> GetFilteredListIncludingInactiveAsync(
        [FromQuery] DataTableListInput<RealtimeStopTimeFilter> input, CancellationToken cancellationToken = default)
        => await AppService.GetFilteredListIncludingInactiveAsync(input, cancellationToken);

    /// <summary>Aktif kaynaktaki durak olcumunu gunceller.</summary>
    /// <param name="id">Guncellenecek durak olcumu kimligi.</param>
    /// <param name="input">Yeni durum, sefer/durak ve olcum alanlari.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { TripScheduleRelationshipId, StopScheduleRelationshipId (int) → lookup baglari;
    /// VehicleStopStatusId, ViolationTypeId (int?) → istege bagli lookup baglari;
    /// GtfsFeedId (Guid), VehicleId (Guid?) → harici kimlikler; ServiceDate (DateOnly), StartTime (string?),
    /// GtfsTripId, GtfsRouteId, GtfsStopId (string), StopSequence (int) → sefer/durak anahtari;
    /// ArrivalTime, DepartureTime (DateTime?), Latitude, Longitude (double?), ReportedDelay (int?) → olcumler;
    /// ArrivalCorrelationId, DepartureCorrelationId, StatusCorrelationId (string?) → kanit kimlikleri }
    /// Response { RealtimeStopTimeDto → guncel alanlar ve lookup adlari }
    /// Kaynak kimligi degismez; islem activity log'a yazilir. Bu endpoint dis akis cekmez.
    /// </remarks>
    [HttpPut(RealtimeStopTimeRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeStopTimes.Manage)]
    [TrackActivity<GtfsRealtimeManagementResource>(
        GtfsRealtimeManagementLocalizationKeys.ActivityLogs.RealtimeStopTimeUpdated,
        ActivityKind.Updated, nameof(RealtimeStopTimeDto.Id))]
    public virtual async Task<Result<RealtimeStopTimeDto>> UpdateAsync(
        Guid id, [FromBody] UpdateRealtimeStopTimeDto input, CancellationToken cancellationToken = default)
        => await AppService.UpdateAsync(id, input, cancellationToken);

    /// <summary>Aktif kaynaktaki durak olcumunu siler.</summary>
    /// <param name="id">Silinecek durak olcumu kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { govde yok; silme kalicidir }
    /// Yalnizca istegin aktif kaynagi degisir; islem activity log'a yazilir. Bu endpoint dis akis cekmez.
    /// </remarks>
    [HttpDelete(RealtimeStopTimeRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeStopTimes.Manage)]
    [TrackActivity<GtfsRealtimeManagementResource>(
        GtfsRealtimeManagementLocalizationKeys.ActivityLogs.RealtimeStopTimeDeleted,
        ActivityKind.Deleted, nameof(RealtimeStopTimeDto.Id))]
    public virtual async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AppService.DeleteAsync(id, cancellationToken);
        return Result.NoContent();
    }
}
