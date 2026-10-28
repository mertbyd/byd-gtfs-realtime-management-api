namespace Byd.GtfsRealtimeManagement.Settings;

/// <summary>
/// GTFS-RT akisinin islenmesindeki durak ve bayatlik esiklerinin ayar adlarini tanimlar.
/// </summary>
// islevi: Aracin durakta sayilma kurali, bayat akis esigi ve erken/gec kalkis toleranslarinin ayar adlarini tanimlar.
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

        // Baslik zamani bundan eski akis bayat sayilir ve islenmez, saniye.
        public const string StaleFeedSeconds = Default + ".StaleFeedSeconds";

        // Planlanan kalkistan bu kadar once kalkan arac erken kalkis ihlali sayilir, saniye.
        public const string EarlyDepartureToleranceSeconds = Default + ".EarlyDepartureToleranceSeconds";

        // Planlanan kalkistan bu kadar sonra kalkan arac gec kalkis ihlali sayilir, saniye.
        public const string LateDepartureToleranceSeconds = Default + ".LateDepartureToleranceSeconds";
    }
}
