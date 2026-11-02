using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

/// <summary>
/// Bir akis adresinin son basarili yanitinin HTTP dogrulayicilari.
/// </summary>
// islevi: Yanitin ETag ve Last-Modified basliklarini adres anahtariyla onbellekte tutar.
// sistemdeki gorevi: Okuyucu bir sonraki cekmede bunlarla kosullu istek atar; icerik degismediyse kaynak 304 doner ve
// yanit yeniden indirilip islenmez.
public class RealtimeFeedValidatorCacheItem
{
    // Yanitin ETag basligi, tirnaklariyla ve zayif isaretiyle.
    public string? ETag { get; set; }

    // Yanitin Last-Modified basligi.
    public DateTimeOffset? LastModified { get; set; }
}
