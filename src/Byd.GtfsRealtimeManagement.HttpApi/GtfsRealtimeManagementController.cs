using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Byd.GtfsRealtimeManagement;

public abstract class GtfsRealtimeManagementController : AbpControllerBase
{
    protected T LazyGetRequiredService<T>() where T : notnull =>
        LazyServiceProvider.LazyGetRequiredService<T>();

    protected GtfsRealtimeManagementController()
    {
        LocalizationResource = typeof(GtfsRealtimeManagementResource);
    }
}
