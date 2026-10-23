using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

/// <summary>
/// Bir kaynagin tek akisinin son cekme ani.
/// </summary>
// islevi: Akis cekildiginde aralik suresiyle onbellege yazilir; kayit durdukca ayni akis yeniden cekilmez.
// sistemdeki gorevi: Organizasyonun aralik ayarini tablo tutmadan uygular; birden fazla instance'ta da ayni sonucu verir.
public class RealtimeFeedFetchCacheItem
{
    // Akisin cekildigi an.
    public DateTime FetchedAt { get; set; }
}
