using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Byd.GtfsRealtimeManagement.Permissions;

public class GtfsRealtimeManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        // var myGroup = context.AddGroup(GtfsRealtimeManagementPermissions.GroupName, L("Permission:GtfsRealtimeManagement"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<GtfsRealtimeManagementResource>(name);
    }
}
