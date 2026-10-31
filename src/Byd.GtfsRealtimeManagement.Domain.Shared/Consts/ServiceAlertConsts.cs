namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: GTFS-RT Alert'in neden, etki ve onem alanlarinin varsayilan kodlarini tanimlar.
// sistemdeki gorevi: Akis alani gondermediginde uyari standarttaki varsayilanla gosterilir.
public static class ServiceAlertConsts
{
    // Alert.cause UNKNOWN_CAUSE, Alert.effect UNKNOWN_EFFECT, Alert.severity_level UNKNOWN_SEVERITY.
    public const int UnknownCauseCode = 1;
    public const int UnknownEffectCode = 8;
    public const int UnknownSeverityCode = 1;
}
