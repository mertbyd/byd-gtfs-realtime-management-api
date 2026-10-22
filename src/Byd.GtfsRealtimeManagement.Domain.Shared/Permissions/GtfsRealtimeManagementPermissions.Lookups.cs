namespace Byd.GtfsRealtimeManagement.Permissions;

// islevi: Realtime lookup okuma ve yonetim izinlerini tanimlar.
// sistemdeki gorevi: Lookup endpointlerinin erisim sozlesmesini sabitler.
public static partial class GtfsRealtimeManagementPermissions
{
    public static class Lookups
    {
        public const string Default = GroupName + ".Lookups";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }
}
