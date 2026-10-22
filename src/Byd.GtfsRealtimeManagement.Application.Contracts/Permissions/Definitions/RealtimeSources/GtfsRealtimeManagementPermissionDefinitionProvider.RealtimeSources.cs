using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Byd.GtfsRealtimeManagement.Permissions;

// islevi: RealtimeSource izinlerini ABP grubuna ekler.
// sistemdeki gorevi: View ve Manage eylemlerini lokalize ederek sunar.
public partial class GtfsRealtimeManagementPermissionDefinitionProvider
{
    // Kaynak izin agacini tanimlar.
    private static void AddRealtimeSourcePermissions(PermissionGroupDefinition group)
    {
        var sources = group.AddPermission(
            GtfsRealtimeManagementPermissions.RealtimeSources.Default,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.RealtimeSources));
        sources.AddChild(
            GtfsRealtimeManagementPermissions.RealtimeSources.View,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.RealtimeSourcesView));
        sources.AddChild(
            GtfsRealtimeManagementPermissions.RealtimeSources.Manage,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.RealtimeSourcesManage));
    }
}
