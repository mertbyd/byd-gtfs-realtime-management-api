using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Extensions.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.LogManagement.Dtos.ExternalServiceLogs;
using Byd.LogManagement.Interfaces.ExternalServiceLogs;
using Volo.Abp.Caching;

namespace Byd.GtfsRealtimeManagement.Adapters.RealtimeFeeds;

// islevi: GTFS-RT adresini ceker, cagriyi ve ham yaniti log modulune kaydeder, yaniti FeedMessage modeline cevirir.
// sistemdeki gorevi: Dis HTTP ve protobuf bagimliligini EF katmanindaki bu adaptorde tutar. Periyodik cekme kosullu istek
// atar: son yanitin ETag/Last-Modified basliklari saklanir, icerik degismediyse kaynak 304 doner ve yanit indirilmez.
// Dogrulayicilar organizasyon ve adresle saklanir; ayni adresi kullanan iki organizasyon birbirinin yanitini gormus sayilmaz.
public class RealtimeFeedReader(
    IHttpClientFactory clientFactory,
    IExternalServiceLogger externalServiceLogger,
    IDistributedCache<RealtimeFeedValidatorCacheItem> validatorCache,
    ICurrentOrganization currentOrganization) : IRealtimeFeedReader
{
    // Adresi kosulsuz indirir ve cozer; ham yanit cozumden once kaydedildigi icin cozulemeyen yanit da log'da kalir.
    public virtual async Task<FeedMessageModel> ReadAsync(
        string sourceName, string url, CancellationToken cancellationToken = default)
        => (await DownloadAsync(sourceName, url, conditional: false, cancellationToken))!.ToFeedMessageModel();

    // Adresi kosullu indirir; icerik degismediyse bos doner.
    public virtual async Task<FeedMessageModel?> ReadIfChangedAsync(
        string sourceName, string url, CancellationToken cancellationToken = default)
        => (await DownloadAsync(sourceName, url, conditional: true, cancellationToken))?.ToFeedMessageModel();

    // Adresi indirir; log modulu cagriyi, suresini ve yanitin base64 halini kaydeder. Kosullu istekte 304 bos yanit olarak
    // kaydedilir ve icerik bos doner; basarili yanitin dogrulayicilari bir sonraki istek icin saklanir.
    private async Task<byte[]?> DownloadAsync(
        string sourceName, string url, bool conditional, CancellationToken cancellationToken = default)
    {
        byte[]? content = null;
        await externalServiceLogger.TrackAsync(
            new ExternalServiceCall(sourceName, url, RealtimeFeedConsts.RequestType),
            async () =>
            {
                using var client = clientFactory.CreateClient(RealtimeFeedConsts.HttpClientName);
                using var request = await CreateRequestAsync(url, conditional, cancellationToken);
                using var response = await client.SendAsync(request, cancellationToken);
                if (response.StatusCode == HttpStatusCode.NotModified)
                    return string.Empty;
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (conditional)
                    await SaveValidatorsAsync(url, response, cancellationToken);
                return Convert.ToBase64String(content);
            },
            cancellationToken);
        return content;
    }

    // Kosullu istekte son yanitin dogrulayicilarini If-None-Match ve If-Modified-Since olarak ekler.
    private async Task<HttpRequestMessage> CreateRequestAsync(
        string url, bool conditional, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!conditional || await validatorCache.GetAsync(ValidatorKey(url), token: cancellationToken) is not { } validators)
            return request;
        if (EntityTagHeaderValue.TryParse(validators.ETag, out var entityTag))
            request.Headers.IfNoneMatch.Add(entityTag);
        request.Headers.IfModifiedSince = validators.LastModified;
        return request;
    }

    // Yanit dogrulayici dondurduyse saklar, dondurmediyse eskisini siler; kaynak dogrulayici vermiyorsa istek kosulsuz kalir.
    private Task SaveValidatorsAsync(string url, HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var validators = new RealtimeFeedValidatorCacheItem
        {
            ETag = response.Headers.ETag?.ToString(),
            LastModified = response.Content.Headers.LastModified
        };
        return validators is { ETag: null, LastModified: null }
            ? validatorCache.RemoveAsync(ValidatorKey(url), token: cancellationToken)
            : validatorCache.SetAsync(ValidatorKey(url), validators,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = RealtimeFeedConsts.ValidatorLifetime },
                token: cancellationToken);
    }

    // Tenant'i ABP onbellegi anahtara kendisi ekler; organizasyon burada eklenir.
    private string ValidatorKey(string url) => $"{currentOrganization.Id}:{url}";
}
