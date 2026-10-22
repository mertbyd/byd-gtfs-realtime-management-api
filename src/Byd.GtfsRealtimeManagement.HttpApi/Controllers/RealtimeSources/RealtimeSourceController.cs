using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Consts;
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
    /// <param name="input">Kaynak adi, adresler ve etkinlik durumu.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { Name (string) → organizasyonda tekil ad; VehiclePositionsUrl, TripUpdatesUrl,
    /// ServiceAlertsUrl (string?) → en az biri dolu mutlak HTTP(S) adresleri; IsActive (bool) → cekim durumu }
    /// Response { RealtimeSourceDto → kimlik, adresler, etkinlik ve audit zamanlari }
    /// Tenant ve organizasyon istek baglamindan gelir. Bu endpoint akis cekmez.
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
    [Authorize(GtfsRealtimeManagementPermissions.RealtimeSources.View)]
    public virtual async Task<Result<PagedResultDto<RealtimeSourceDto>>> GetFilteredListAsync(
        [FromQuery] DataTableListInput<RealtimeSourceFilter> input, CancellationToken cancellationToken = default)
        => await AppService.GetFilteredListAsync(input, cancellationToken);

    /// <summary>Kimligi verilen realtime kaynagini gunceller.</summary>
    /// <param name="id">Guncellenecek kaynak kimligi.</param>
    /// <param name="input">Yeni ad, adresler ve etkinlik durumu.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request { Name (string) → organizasyonda tekil ad; VehiclePositionsUrl, TripUpdatesUrl,
    /// ServiceAlertsUrl (string?) → en az biri dolu mutlak HTTP(S) adresleri; IsActive (bool) → cekim durumu }
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
}
