using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Byd.GtfsRealtimeManagement.Permissions;

// islevi: Durak olcumu izinlerini ABP agacina ekler.
// sistemdeki gorevi: View ve Manage haklarini lokalize eder.
public partial class GtfsRealtimeManagementPermissionDefinitionProvider
{
    // Durak olcumu izin dugumlerini tanimlar.
    private static void AddRealtimeStopTimePermissions(PermissionGroupDefinition group)
    {
        var times = group.AddPermission(GtfsRealtimeManagementPermissions.RealtimeStopTimes.Default,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.RealtimeStopTimes));
        times.AddChild(GtfsRealtimeManagementPermissions.RealtimeStopTimes.View,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.RealtimeStopTimesView));
        times.AddChild(GtfsRealtimeManagementPermissions.RealtimeStopTimes.Manage,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.RealtimeStopTimesManage));
    }
}
