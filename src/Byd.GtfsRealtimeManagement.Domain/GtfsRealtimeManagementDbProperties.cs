using Byd.GtfsRealtimeManagement.Constants;

namespace Byd.GtfsRealtimeManagement;

public static class GtfsRealtimeManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "GtfsRealtimeManagement";

    public static string? DbSchema { get; set; } = GtfsRealtimeManagementDatabaseConstants.SchemaName;

    public const string ConnectionStringName = "GtfsRealtimeManagement";
}
