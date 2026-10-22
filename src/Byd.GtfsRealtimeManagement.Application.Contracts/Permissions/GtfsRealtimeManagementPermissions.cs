using Volo.Abp.Reflection;

namespace Byd.GtfsRealtimeManagement.Permissions;

public class GtfsRealtimeManagementPermissions
{
    // public const string GroupName = "GtfsRealtimeManagement";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(GtfsRealtimeManagementPermissions));
    }
}
