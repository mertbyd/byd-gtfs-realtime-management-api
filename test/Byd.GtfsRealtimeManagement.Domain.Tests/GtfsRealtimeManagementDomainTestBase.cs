using Volo.Abp.Modularity;

namespace Byd.GtfsRealtimeManagement;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class GtfsRealtimeManagementDomainTestBase<TStartupModule> : GtfsRealtimeManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
