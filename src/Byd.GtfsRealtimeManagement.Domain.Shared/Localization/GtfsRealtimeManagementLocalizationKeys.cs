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
        public const string RealtimeSources = "Permission:RealtimeSources";
        public const string RealtimeSourcesView = "Permission:RealtimeSources.View";
        public const string RealtimeSourcesManage = "Permission:RealtimeSources.Manage";
    }

    public static class ActivityLogs
    {
        public const string LookupUpdated = "ActivityLogs:LookupUpdated";
        public const string LookupDeleted = "ActivityLogs:LookupDeleted";
        public const string RealtimeSourceUpdated = "ActivityLogs:RealtimeSourceUpdated";
        public const string RealtimeSourceDeleted = "ActivityLogs:RealtimeSourceDeleted";
    }
}
