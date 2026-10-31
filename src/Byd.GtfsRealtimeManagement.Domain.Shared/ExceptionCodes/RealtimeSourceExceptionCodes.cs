namespace Byd.GtfsRealtimeManagement.ExceptionCodes;

// islevi: Kaynak kapsam ve girdi hatalarinin kodlarini tanimlar.
// sistemdeki gorevi: Beklenen hatalari lokalizasyon sozlesmesine baglar.
public static class RealtimeSourceExceptionCodes
{
    public const string ScopeRequired = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0001";
    public const string NameRequired = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0002";
    public const string UrlRequired = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0003";
    public const string UrlInvalid = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0004";
    public const string FeedUrlNotConfigured = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0005";
    public const string ActiveSourceRequired = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0006";
    public const string GtfsFeedRequired = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0007";
    public const string FeedPlanMismatch = GtfsRealtimeManagementErrorCodes.RealtimeSourceGroup + "0008";
}
