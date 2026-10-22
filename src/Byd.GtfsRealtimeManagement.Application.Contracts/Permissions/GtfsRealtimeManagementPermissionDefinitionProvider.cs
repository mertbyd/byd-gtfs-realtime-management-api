using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Byd.GtfsRealtimeManagement.Permissions;

public partial class GtfsRealtimeManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            GtfsRealtimeManagementPermissions.GroupName,
            L(GtfsRealtimeManagementLocalizationKeys.Permissions.Group));
        AddLookupPermissions(group);
        AddRealtimeSourcePermissions(group);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<GtfsRealtimeManagementResource>(name);
    }
}
