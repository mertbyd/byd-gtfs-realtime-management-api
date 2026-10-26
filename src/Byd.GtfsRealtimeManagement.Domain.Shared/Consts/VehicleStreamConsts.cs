namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: Arac konumlarinin log modulunun web bildirimiyle giderken tasidigi basligi ve dil anahtarlarini tanimlar.
// sistemdeki gorevi: Istemci notificationReceived mesajini bu baslikla ayirt eder; arac listesi aciklamada JSON olarak gelir.
public static class VehicleStreamConsts
{
    // Her arac konumu cekmesinden sonra organizasyona giden yayin bildiriminin basligi.
    public const string NotificationTitle = "VehiclePositions";

    // Aciklama dil koduna gore secilir; arac listesi iki dilde de aynidir.
    public const string TurkishLanguageCode = "tr";
    public const string EnglishLanguageCode = "en";
}
