using Volo.Abp.Reflection;

namespace Byd.GtfsRealtimeManagement.Permissions;

public static partial class GtfsRealtimeManagementPermissions
{
    public const string GroupName = "GtfsRealtimeManagement";

    public static string[] GetAll() =>
        ReflectionHelper.GetPublicConstantsRecursively(typeof(GtfsRealtimeManagementPermissions));
}
