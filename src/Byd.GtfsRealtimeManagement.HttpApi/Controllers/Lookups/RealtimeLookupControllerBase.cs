using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Byd.GtfsRealtimeManagement.Localization;
using Byd.GtfsRealtimeManagement.Permissions;
using Byd.GtfsRealtimeManagement.Services.Lookups;
using Byd.LogManagement.ActivityLogs;
using Byd.LogManagement.Attributes;
using SystemStandards.Results;

namespace Byd.GtfsRealtimeManagement.Controllers.Lookups;

// islevi: Realtime lookup turlerinin ortak HTTP eylemlerini, yetki ve activity log metadatasini sunar.
// sistemdeki gorevi: Her lookup controller'inin yalniz rotasini bildirip CRUD uclarini bu tabandan almasini saglar.
public abstract class RealtimeLookupControllerBase<TAppService> : GtfsRealtimeManagementController
    where TAppService : IRealtimeLookupCrudAppService
{
    private TAppService AppService => LazyGetRequiredService<TAppService>();

    /// <summary>Yeni lookup kaydi olusturur.</summary>
    /// <param name="input">Kod ve gorunen ad.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request {
    ///   Code (int)    → Turde tekil kod; sifir veya daha buyuk
    ///   Name (string) → Gorunen ad; zorunlu, bas ve sondaki bosluklar kirpilir
    /// }
    /// Response { Id (int) → Yeni kaydin kimligi; Code (int) → Kayitli kod; Name (string) → Kayitli ad }
    /// Kayit host kapsamindadir; tum tenant ve organizasyonlar ayni listeyi gorur. Kod turde baska bir kayitta
    /// varsa hata doner. Bu endpoint diger lookup turlerine dokunmaz.
    /// </remarks>
    [HttpPost]
    [Authorize(GtfsRealtimeManagementPermissions.Lookups.Manage)]
    public virtual async Task<Result<RealtimeLookupDto>> CreateAsync(
        [FromBody] CreateRealtimeLookupDto input, CancellationToken cancellationToken = default)
        => await AppService.CreateAsync(input, cancellationToken);

    /// <summary>Kimligi verilen lookup kaydini getirir.</summary>
    /// <param name="id">Lookup kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response { Id (int) → Kayit kimligi; Code (int) → Lookup kodu; Name (string) → Gorunen ad }
    /// Kayit yoksa bulunamadi hatasi doner. Bu endpoint kayit yazmaz.
    /// </remarks>
    [HttpGet(RealtimeLookupRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.Lookups.View)]
    public virtual async Task<Result<RealtimeLookupDto>> GetAsync(int id, CancellationToken cancellationToken = default)
        => await AppService.GetAsync(id, cancellationToken);

    /// <summary>Lookup turunun tum kayitlarini getirir.</summary>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response {
    ///   [] (RealtimeLookupDto) → Turun kayitlari, koda gore sirali
    ///     Id   (int)    → Kayit kimligi
    ///     Code (int)    → Lookup kodu
    ///     Name (string) → Gorunen ad
    /// }
    /// Liste sayfasizdir; kayit yoksa bos liste doner. Bu endpoint kayit yazmaz.
    /// </remarks>
    [HttpGet]
    [Authorize(GtfsRealtimeManagementPermissions.Lookups.View)]
    public virtual async Task<Result<List<RealtimeLookupDto>>> GetListAsync(CancellationToken cancellationToken = default)
        => await AppService.GetListAsync(cancellationToken);

    /// <summary>Kimligi verilen lookup kaydini gunceller.</summary>
    /// <param name="id">Lookup kimligi.</param>
    /// <param name="input">Yeni kod ve gorunen ad.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Request {
    ///   Code (int)    → Yeni kod; turde tekil, sifir veya daha buyuk
    ///   Name (string) → Yeni gorunen ad; zorunlu
    /// }
    /// Response { Id (int) → Kayit kimligi; Code (int) → Guncel kod; Name (string) → Guncel ad }
    /// Kimlik degismez. Kod turde baska bir kayitta varsa hata doner. Degisiklik activity log'a yazilir.
    /// </remarks>
    [HttpPut(RealtimeLookupRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.Lookups.Manage)]
    [TrackActivity<GtfsRealtimeManagementResource>(
        GtfsRealtimeManagementLocalizationKeys.ActivityLogs.LookupUpdated,
        ActivityKind.Updated, nameof(RealtimeLookupDto.Id))]
    public virtual async Task<Result<RealtimeLookupDto>> UpdateAsync(
        int id, [FromBody] UpdateRealtimeLookupDto input, CancellationToken cancellationToken = default)
        => await AppService.UpdateAsync(id, input, cancellationToken);

    /// <summary>Kimligi verilen lookup kaydini siler.</summary>
    /// <param name="id">Lookup kimligi.</param>
    /// <param name="cancellationToken">Istek iptal belirteci.</param>
    /// <remarks>
    /// Response govdesi yoktur; kayit kalici olarak silinir ve silme activity log'a yazilir. Silinen satir
    /// varsayilan satirlardan biriyse ve kodu bos kaldiysa uygulama bir sonraki acilista onu yeniden ekler.
    /// Bu endpoint diger lookup turlerine dokunmaz.
    /// </remarks>
    [HttpDelete(RealtimeLookupRoutes.ById)]
    [Authorize(GtfsRealtimeManagementPermissions.Lookups.Manage)]
    [TrackActivity<GtfsRealtimeManagementResource>(
        GtfsRealtimeManagementLocalizationKeys.ActivityLogs.LookupDeleted,
        ActivityKind.Deleted, nameof(RealtimeLookupDto.Id))]
    public virtual async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await AppService.DeleteAsync(id, cancellationToken);
        return Result.NoContent();
    }
}
