using Byd.CoreExtensions.Repositories;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: ViolationType lookup repository sozlesmesini core EF tabanina baglar.
// sistemdeki gorevi: Lookup silmesinden once core'un iliski kontrolunu kullanilabilir kilar.
[ExposeServices(typeof(IRealtimeLookupRepository<ViolationType>))]
public class EfCoreViolationTypeRepository(IDbContextProvider<GtfsRealtimeManagementDbContext> dbContextProvider)
    : EfExtendedRepository<GtfsRealtimeManagementDbContext, ViolationType, int>(dbContextProvider), IRealtimeLookupRepository<ViolationType>
{
}
