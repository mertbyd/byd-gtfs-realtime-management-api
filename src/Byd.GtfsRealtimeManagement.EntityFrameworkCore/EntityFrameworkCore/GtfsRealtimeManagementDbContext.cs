using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.Extensions;
using Volo.Abp.Data;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

[ConnectionStringName(GtfsRealtimeManagementDbProperties.ConnectionStringName)]
public class GtfsRealtimeManagementDbContext : BydDbContext<GtfsRealtimeManagementDbContext>, IGtfsRealtimeManagementDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public GtfsRealtimeManagementDbContext(DbContextOptions<GtfsRealtimeManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(GtfsRealtimeManagementDbContext).Assembly);

        builder.ConfigureGtfsRealtimeManagement();

        builder.ApplySnakeCaseNaming();
    }
}
