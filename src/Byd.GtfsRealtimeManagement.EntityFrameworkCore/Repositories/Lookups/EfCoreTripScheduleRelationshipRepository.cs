using Byd.CoreExtensions.Repositories;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: TripScheduleRelationship lookup repository sozlesmesini core EF tabanina baglar.
// sistemdeki gorevi: Lookup silmesinden once core'un iliski kontrolunu kullanilabilir kilar.
[ExposeServices(typeof(IRealtimeLookupRepository<TripScheduleRelationship>))]
public class EfCoreTripScheduleRelationshipRepository(IDbContextProvider<GtfsRealtimeManagementDbContext> dbContextProvider)
    : EfExtendedRepository<GtfsRealtimeManagementDbContext, TripScheduleRelationship, int>(dbContextProvider), IRealtimeLookupRepository<TripScheduleRelationship>
{
}
