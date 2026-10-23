using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Extensions.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Byd.LogManagement.Dtos.ExternalServiceLogs;
using Byd.LogManagement.Interfaces.ExternalServiceLogs;

namespace Byd.GtfsRealtimeManagement.Adapters.RealtimeFeeds;

// islevi: GTFS-RT adresini ceker, cagriyi ve ham yaniti log modulune kaydeder, yaniti FeedMessage modeline cevirir.
// sistemdeki gorevi: Dis HTTP ve protobuf bagimliligini EF katmanindaki bu adaptorde tutar.
public class RealtimeFeedReader(
    IHttpClientFactory clientFactory,
    IExternalServiceLogger externalServiceLogger) : IRealtimeFeedReader
{
    // Adresi indirir ve cozer; ham yanit cozumden once kaydedildigi icin cozulemeyen yanit da log'da kalir.
    public virtual async Task<FeedMessageModel> ReadAsync(
        string sourceName, string url, CancellationToken cancellationToken = default)
    {
        var content = await DownloadAsync(sourceName, url, cancellationToken);
        return content.ToFeedMessageModel();
    }

    // Adresi indirir; log modulu cagriyi, suresini ve yanitin base64 halini kaydeder.
    private async Task<byte[]> DownloadAsync(
        string sourceName, string url, CancellationToken cancellationToken = default)
    {
        byte[] content = [];
        await externalServiceLogger.TrackAsync(
            new ExternalServiceCall(sourceName, url, RealtimeFeedConsts.RequestType),
            async () =>
            {
                using var client = clientFactory.CreateClient(RealtimeFeedConsts.HttpClientName);
                content = await client.GetByteArrayAsync(url, cancellationToken);
                return Convert.ToBase64String(content);
            },
            cancellationToken);
        return content;
    }
}
