namespace Byd.GtfsRealtimeManagement.Permissions;

// islevi: Kaynak okuma ve yonetim izinlerini tanimlar.
// sistemdeki gorevi: CRUD erisim sozlesmesini sabitler.
public static partial class GtfsRealtimeManagementPermissions
{
    public static class RealtimeSources
    {
        public const string Default = GroupName + ".RealtimeSources";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }
}
