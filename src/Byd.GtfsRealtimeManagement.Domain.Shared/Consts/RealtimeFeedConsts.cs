using System;
using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: GTFS-RT cekmesinin HTTP istemcisini, istek tipini, zaman asimini, yanit sinirini ve isci turunu tanimlar.
// sistemdeki gorevi: Okuyucu, modul kaydi ve cekme iscisi ayni sabitleri kullanir.
public static class RealtimeFeedConsts
{
    public const string HttpClientName = "RealtimeFeedReader";
    public const string RequestType = "GET";
    public static readonly IReadOnlyList<string> SupportedVersions = Array.AsReadOnly(new[] { "1.0", "2.0" });

    // Tek yanitin en buyuk boyutu; Vilnius yanitlari 50-110 KB.
    public const long MaxResponseBytes = 10L * 1024 * 1024;

    // Iscinin tur araligi ve birden fazla instance'ta tek calismasini saglayan kilit; her akisin asil araligi ayardan okunur.
    public const int WorkerPeriodSeconds = 5;
    public const string FetchLockName = "GtfsRealtimeManagement.RealtimeFeedFetch";

    // Yanit en fazla bu kadar beklenir; en kisa cekme araligindan kisadir.
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);
}
