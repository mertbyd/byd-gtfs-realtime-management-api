namespace Byd.GtfsRealtimeManagement.ExceptionCodes;

// islevi: Cekilen GTFS-RT yanitinin cozulememe ve desteklenmeme hatalarinin kodlarini tanimlar.
// sistemdeki gorevi: Okuyucunun protobuf ve baslik hatalarini lokalizasyon sozlesmesine baglar.
public static class RealtimeFeedExceptionCodes
{
    public const string FeedInvalid = GtfsRealtimeManagementErrorCodes.RealtimeFeedGroup + "0001";
    public const string FeedNotSupported = GtfsRealtimeManagementErrorCodes.RealtimeFeedGroup + "0002";
}
