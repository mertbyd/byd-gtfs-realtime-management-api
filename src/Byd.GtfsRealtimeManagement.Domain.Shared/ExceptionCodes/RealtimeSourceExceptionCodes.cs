namespace Byd.GtfsRealtimeManagement.ExceptionCodes;

// islevi: Kaynak kapsam ve girdi hatalarinin kodlarini tanimlar.
// sistemdeki gorevi: Beklenen hatalari lokalizasyon sozlesmesine baglar.
public static class RealtimeSourceExceptionCodes
{
    public const string ScopeRequired = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0001";
    public const string NameRequired = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0002";
    public const string UrlRequired = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0003";
    public const string UrlInvalid = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0004";
}
