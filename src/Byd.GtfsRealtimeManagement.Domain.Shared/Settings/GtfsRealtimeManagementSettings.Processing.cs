namespace Byd.GtfsRealtimeManagement.Settings;

/// <summary>
/// GTFS-RT akisinin islenmesindeki durak, bayatlik ve ihlal esiklerinin ayar adlarini tanimlar.
/// </summary>
// islevi: Aracin durakta sayilma kurali, bayat akis esikleri, erken/gec kalkis toleranslari ve yapilmayan sefer bekleme
// suresinin ayar adlarini tanimlar.
// sistemdeki gorevi: Isleme adimi bu adlari kaynagin kapsaminda okur; organizasyon, tenant ve global seviyede
// degistirilebilir.
public partial class GtfsRealtimeManagementSettings
{
    public static class Processing
    {
        public const string Default = GroupName + ".Processing";

        // Aracin duraga girmis sayildigi en buyuk uzaklik, metre.
        public const string StopRadiusMeters = Default + ".StopRadiusMeters";

        // Aracin duruyor sayildigi en buyuk hiz, metre/saniye (GTFS-RT Position.speed birimi).
        public const string StoppedSpeedMetersPerSecond = Default + ".StoppedSpeedMetersPerSecond";

        // Baslik zamani bundan eski arac konumu ve sefer guncellemesi akisi bayat sayilir ve islenmez, saniye.
        public const string StaleFeedSeconds = Default + ".StaleFeedSeconds";

        // Baslik zamani bundan eski servis uyarisi akisi bayat sayilir ve islenmez, saniye.
        public const string ServiceAlertsStaleFeedSeconds = Default + ".ServiceAlertsStaleFeedSeconds";

        // Planlanan kalkistan bu kadar once kalkan arac erken kalkis ihlali sayilir, saniye.
        public const string EarlyDepartureToleranceSeconds = Default + ".EarlyDepartureToleranceSeconds";

        // Planlanan kalkistan bu kadar sonra kalkan arac gec kalkis ihlali sayilir, saniye.
        public const string LateDepartureToleranceSeconds = Default + ".LateDepartureToleranceSeconds";

        // Seferin son planlanan varisindan bu kadar sonra hala arac eslesmediyse sefer yapilmamis sayilir, dakika.
        public const string MissedTripGraceMinutes = Default + ".MissedTripGraceMinutes";
    }
}
