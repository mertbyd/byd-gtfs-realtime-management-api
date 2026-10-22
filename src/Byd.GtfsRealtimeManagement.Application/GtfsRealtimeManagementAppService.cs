using Byd.GtfsRealtimeManagement.Localization;
using Volo.Abp.Application.Services;

namespace Byd.GtfsRealtimeManagement;

public abstract class GtfsRealtimeManagementAppService : ApplicationService
{
    protected T LazyGetRequiredService<T>() where T : notnull =>
        LazyServiceProvider.LazyGetRequiredService<T>();

    protected GtfsRealtimeManagementAppService()
    {
        LocalizationResource = typeof(GtfsRealtimeManagementResource);
        ObjectMapperContext = typeof(GtfsRealtimeManagementApplicationModule);
    }
}
