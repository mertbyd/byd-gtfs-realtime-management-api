using System;
using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: GTFS-RT cekmesinin HTTP istemcisini, istek tipini, zaman asimini, yanit sinirini, isci turunu ve akis sagligi
// bildiriminin dillerini tanimlar.
// sistemdeki gorevi: Okuyucu, modul kaydi, cekme iscisi ve cekme adimi ayni sabitleri kullanir.
public static class RealtimeFeedConsts
{
    public const string HttpClientName = "RealtimeFeedReader";
    public const string RequestType = "GET";
    public static readonly IReadOnlyList<string> SupportedVersions = Array.AsReadOnly(new[] { "1.0", "2.0" });

    // Kosullu istegin dogrulayicilari (ETag, Last-Modified) en fazla bu kadar saklanir; sonra istek kosulsuz gider.
    public static readonly TimeSpan ValidatorLifetime = TimeSpan.FromHours(1);

    // Akis sagligi bildiriminin metin dilleri; metin kaynak ve akis adidir, istemci basligi koduyla cevirir.
    public const string TurkishLanguageCode = "tr";
    public const string EnglishLanguageCode = "en";

    // Tek yanitin en buyuk boyutu; Vilnius yanitlari 50-110 KB.
    public const long MaxResponseBytes = 10L * 1024 * 1024;

    // Iscinin tur araligi ve birden fazla instance'ta tek calismasini saglayan kilit; her akisin asil araligi ayardan okunur.
    public const int WorkerPeriodSeconds = 5;
    public const string FetchLockName = "GtfsRealtimeManagement.RealtimeFeedFetch";

    // Yanit en fazla bu kadar beklenir; en kisa cekme araligindan kisadir.
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);
}
