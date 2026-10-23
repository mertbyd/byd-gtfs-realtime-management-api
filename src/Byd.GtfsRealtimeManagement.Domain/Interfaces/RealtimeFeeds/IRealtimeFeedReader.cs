using System.Threading;
using System.Threading.Tasks;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

namespace Byd.GtfsRealtimeManagement.Interfaces.RealtimeFeeds;

// islevi: Bir GTFS-RT adresini ceker, cagriyi ham yanitla log modulune kaydeder ve yaniti FeedMessage modeline cevirir.
// sistemdeki gorevi: Cekme iscisi ile dis HTTP/protobuf entegrasyonu arasindaki sozlesmedir; uygulamasi EF katmanindaki
// adaptordedir.
public interface IRealtimeFeedReader
{
    Task<FeedMessageModel> ReadAsync(string sourceName, string url, CancellationToken cancellationToken = default);
}
