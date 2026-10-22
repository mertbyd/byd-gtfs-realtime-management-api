using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.EntityFrameworkCore;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

public class GtfsRealtimeManagementDbContextFactory
    : CoreExtensionsDesignTimeDbContextFactory<GtfsRealtimeManagementDbContext>
{
    protected override string ConnectionStringName => GtfsRealtimeManagementDbProperties.ConnectionStringName;

    protected override GtfsRealtimeManagementDbContext CreateInstance(
        DbContextOptions<GtfsRealtimeManagementDbContext> options)
    {
        return new(options);
    }
}
