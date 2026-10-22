using System;
using Byd.CoreExtensions.Repositories;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.Interfaces.RealtimeSources;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: RealtimeSource repository sozlesmesini core EF tabanina baglar.
// sistemdeki gorevi: Filtreli sorgu ve kaliciligi organizasyon veri filtresiyle sunar.
[ExposeServices(typeof(IRealtimeSourceRepository))]
public class EfCoreRealtimeSourceRepository(
    IDbContextProvider<GtfsRealtimeManagementDbContext> dbContextProvider)
    : EfExtendedRepository<GtfsRealtimeManagementDbContext, RealtimeSource, Guid>(dbContextProvider),
        IRealtimeSourceRepository
{
}
