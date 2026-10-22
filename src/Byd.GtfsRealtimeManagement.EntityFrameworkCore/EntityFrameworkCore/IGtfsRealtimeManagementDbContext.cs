using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

[ConnectionStringName(GtfsRealtimeManagementDbProperties.ConnectionStringName)]
public interface IGtfsRealtimeManagementDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
