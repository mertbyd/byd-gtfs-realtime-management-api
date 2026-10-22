using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Byd.GtfsRealtimeManagement;

public abstract class GtfsRealtimeManagementController : AbpControllerBase
{
    protected GtfsRealtimeManagementController()
    {
        LocalizationResource = typeof(GtfsRealtimeManagementResource);
    }
}
