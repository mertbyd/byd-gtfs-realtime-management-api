using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class GtfsRealtimeManagementApplicationTestBase<TStartupModule> : GtfsRealtimeManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
