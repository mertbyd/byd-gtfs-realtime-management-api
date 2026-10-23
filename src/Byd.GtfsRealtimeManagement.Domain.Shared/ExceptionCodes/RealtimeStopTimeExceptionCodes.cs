namespace Byd.GtfsRealtimeManagement.ExceptionCodes;

// islevi: Durak olcumu istek hatalarinin kodlarini tanimlar.
// sistemdeki gorevi: FluentValidation hatalarini lokalize eder.
public static class RealtimeStopTimeExceptionCodes
{
    public const string FeedRequired = GtfsRealtimeManagementErrorCodes.RealtimeStopTimeGroup + "0001";
    public const string RelationshipRequired = GtfsRealtimeManagementErrorCodes.RealtimeStopTimeGroup + "0002";
    public const string ServiceDateRequired = GtfsRealtimeManagementErrorCodes.RealtimeStopTimeGroup + "0003";
    public const string GtfsIdRequired = GtfsRealtimeManagementErrorCodes.RealtimeStopTimeGroup + "0004";
    public const string StopSequenceInvalid = GtfsRealtimeManagementErrorCodes.RealtimeStopTimeGroup + "0005";
    public const string CoordinatesInvalid = GtfsRealtimeManagementErrorCodes.RealtimeStopTimeGroup + "0006";
    public const string StartTimeInvalid = GtfsRealtimeManagementErrorCodes.RealtimeStopTimeGroup + "0007";
}
