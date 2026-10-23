using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Controllers.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Permissions;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.RealtimeStopTimes;

// islevi: Durak olcumu migration ve kaynak FK sozlesmesini dogrular.
// sistemdeki gorevi: Islenmis tablo yapisindaki veri butunlugu riskini test eder.
public class RealtimeStopTimeModelTests
{
    [Fact]
    public async Task Migration_CreatesStopTimeTableAndUniqueTripStopIndex()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new GtfsRealtimeManagementDbContext(
            new DbContextOptionsBuilder<GtfsRealtimeManagementDbContext>().UseSqlite(connection).Options);
        await context.Database.MigrateAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'RealtimeStopTimes'";
        Assert.Equal(RealtimeStopTimeConsts.TableName, await command.ExecuteScalarAsync());
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'IX_RealtimeStopTimes_Source_Service_Trip_Start_Sequence'";
        Assert.Equal(RealtimeStopTimeConsts.TripStopIndex, await command.ExecuteScalarAsync());
    }

    [Fact]
    public void Model_RestrictsSourceDelete_AndMakesTripStopUnique()
    {
        using var context = new GtfsRealtimeManagementDbContext(
            new DbContextOptionsBuilder<GtfsRealtimeManagementDbContext>()
                .UseSqlite("Data Source=:memory:").Options);
        var entity = context.Model.FindEntityType(typeof(RealtimeStopTime));
        Assert.NotNull(entity);
        var sourceFk = Assert.Single(entity.GetForeignKeys(), key => key.PrincipalEntityType.ClrType == typeof(RealtimeSource));
        Assert.Equal(DeleteBehavior.Restrict, sourceFk.DeleteBehavior);
        var unique = Assert.Single(entity.GetIndexes(), index => index.IsUnique);
        Assert.Equal(new[] { "RealtimeSourceId", "ServiceDate", "GtfsTripId", "StartTime", "StopSequence" },
            unique.Properties.Select(property => property.Name).ToArray());
        Assert.Equal(false, unique.FindAnnotation("Npgsql:NullsDistinct")?.Value);
    }

    [Fact]
    public void Controller_ReservesHistoryAndWritesForManagePermission()
    {
        var actions = typeof(RealtimeStopTimeController).GetMethods()
            .Where(method => method.DeclaringType == typeof(RealtimeStopTimeController)).ToArray();
        Assert.Equal(7, actions.Length);
        foreach (var action in actions)
        {
            var authorize = (Microsoft.AspNetCore.Authorization.AuthorizeAttribute?)Attribute.GetCustomAttribute(
                action, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute));
            Assert.NotNull(authorize);
            var expected = action.Name is nameof(RealtimeStopTimeController.GetAsync)
                or nameof(RealtimeStopTimeController.GetFilteredListAsync)
                ? GtfsRealtimeManagementPermissions.RealtimeStopTimes.View
                : GtfsRealtimeManagementPermissions.RealtimeStopTimes.Manage;
            Assert.Equal(expected, authorize.Policy);
        }
    }
}
