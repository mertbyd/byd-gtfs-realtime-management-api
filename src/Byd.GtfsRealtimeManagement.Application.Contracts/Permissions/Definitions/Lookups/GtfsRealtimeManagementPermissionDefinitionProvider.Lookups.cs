using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Byd.GtfsRealtimeManagement.Permissions;

// islevi: Realtime lookup izinlerini ABP grubuna ekler.
// sistemdeki gorevi: View ve Manage eylemlerini localized olarak sunar.
public partial class GtfsRealtimeManagementPermissionDefinitionProvider
{
    private static void AddLookupPermissions(PermissionGroupDefinition group)
    {
        var lookups = group.AddPermission(
            GtfsRealtimeManagementPermissions.Lookups.Default,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.Lookups));
        lookups.AddChild(
            GtfsRealtimeManagementPermissions.Lookups.View,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.LookupsView));
        lookups.AddChild(
            GtfsRealtimeManagementPermissions.Lookups.Manage,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.LookupsManage));
    }
}
