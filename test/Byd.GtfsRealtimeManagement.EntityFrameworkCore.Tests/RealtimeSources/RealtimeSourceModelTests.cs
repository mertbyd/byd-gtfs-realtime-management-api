using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Controllers.RealtimeSources;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.Permissions;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.RealtimeSources;

public class RealtimeSourceModelTests
{
    [Fact]
    public async Task Migration_CreatesSourceTableAndUniqueNameIndex()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var dbContext = new GtfsRealtimeManagementDbContext(
            new DbContextOptionsBuilder<GtfsRealtimeManagementDbContext>().UseSqlite(connection).Options);
        await dbContext.Database.MigrateAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'RealtimeSources'";
        Assert.Equal(RealtimeSourceConsts.TableName, await command.ExecuteScalarAsync());
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'IX_RealtimeSources_Scope_Name'";
        Assert.Equal(RealtimeSourceConsts.NameIndex, await command.ExecuteScalarAsync());
    }

    [Fact]
    public void Model_HasOnlyExpectedScopeAndNameIndex()
    {
        using var dbContext = new GtfsRealtimeManagementDbContext(
            new DbContextOptionsBuilder<GtfsRealtimeManagementDbContext>()
                .UseSqlite("Data Source=:memory:").Options);
        var entity = dbContext.Model.FindEntityType(typeof(RealtimeSource));
        Assert.NotNull(entity);
        Assert.Equal(RealtimeSourceConsts.TableName, entity.GetTableName());
        var index = Assert.Single(entity.GetIndexes());
        Assert.True(index.IsUnique);
        Assert.Equal(new[] { "TenantId", "OrganizationId", "Name" },
            index.Properties.Select(property => property.Name).ToArray());
    }

    [Fact]
    public void Controller_UsesSeparateReadAndManagePermissions()
    {
        var actions = typeof(RealtimeSourceController).GetMethods()
            .Where(method => method.DeclaringType == typeof(RealtimeSourceController)).ToArray();
        Assert.Equal(5, actions.Length);
        Assert.Equal(RealtimeSourceRoutes.Root,
            ((Microsoft.AspNetCore.Mvc.RouteAttribute)Attribute.GetCustomAttribute(
                typeof(RealtimeSourceController), typeof(Microsoft.AspNetCore.Mvc.RouteAttribute))!).Template);
        foreach (var action in actions)
        {
            var authorize = (Microsoft.AspNetCore.Authorization.AuthorizeAttribute?)Attribute.GetCustomAttribute(
                action, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute));
            Assert.NotNull(authorize);
            var expected = action.Name is nameof(RealtimeSourceController.GetAsync)
                or nameof(RealtimeSourceController.GetFilteredListAsync)
                ? GtfsRealtimeManagementPermissions.RealtimeSources.View
                : GtfsRealtimeManagementPermissions.RealtimeSources.Manage;
            Assert.Equal(expected, authorize.Policy);
        }
    }
}
