namespace Byd.GtfsRealtimeManagement.Permissions;

// islevi: Durak olcumu okuma ve yonetim izinlerini tanimlar.
// sistemdeki gorevi: HTTP erisim sozlesmesini sabitler.
public static partial class GtfsRealtimeManagementPermissions
{
    public static class RealtimeStopTimes
    {
        public const string Default = GroupName + ".RealtimeStopTimes";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }
}
