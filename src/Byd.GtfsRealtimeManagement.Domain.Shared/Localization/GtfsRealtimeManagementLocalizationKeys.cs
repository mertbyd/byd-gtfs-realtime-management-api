namespace Byd.GtfsRealtimeManagement.Localization;

// islevi: Realtime lookup izin ve activity log metin anahtarlarini tanimlar.
// sistemdeki gorevi: Gorunen metinleri sabit sozlesmelere baglar.
public static class GtfsRealtimeManagementLocalizationKeys
{
    public static class Permissions
    {
        public const string Group = "Permission:GtfsRealtimeManagement";
        public const string Lookups = "Permission:Lookups";
        public const string LookupsView = "Permission:Lookups.View";
        public const string LookupsManage = "Permission:Lookups.Manage";
    }

    public static class ActivityLogs
    {
        public const string LookupUpdated = "ActivityLogs:LookupUpdated";
        public const string LookupDeleted = "ActivityLogs:LookupDeleted";
    }
}
