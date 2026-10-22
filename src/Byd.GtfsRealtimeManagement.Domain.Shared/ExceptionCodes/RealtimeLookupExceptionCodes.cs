namespace Byd.GtfsRealtimeManagement.ExceptionCodes;

// islevi: Realtime lookup alan ve is kurali hatalarinin kodlarini tanimlar.
// sistemdeki gorevi: Lookup sozlesmesinin ihlallerini lokalize edilebilir kodlara baglar.
public static class RealtimeLookupExceptionCodes
{
    public const string NameRequired = GtfsRealtimeManagementErrorCodes.LookupGroup + "0001";
    public const string CodeInvalid = GtfsRealtimeManagementErrorCodes.LookupGroup + "0002";
    public const string CodeAlreadyExists = GtfsRealtimeManagementErrorCodes.LookupGroup + "0003";
}
