using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

public static class GtfsRealtimeManagementDbContextModelCreatingExtensions
{
    public static void ConfigureGtfsRealtimeManagement(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
