using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Interfaces.Lookups;
using Byd.GtfsRealtimeManagement.Managers.Lookups;
using Byd.GtfsRealtimeManagement.Mappers.Lookups;
using Byd.GtfsRealtimeManagement.Models.Lookups;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.Lookups;

public class RealtimeLookupTests
{
    [Fact]
    public void Definitions_MatchRealtimeDatabaseDesign()
    {
        Assert.Equal(new (int, int, string)[]
        {
            (1, 0, "Planlı"), (2, 1, "Eklendi"), (3, 2, "Plansız"), (4, 3, "İptal"),
            (5, 5, "Yerine konan"), (6, 6, "Kopya"), (7, 7, "Silindi"), (8, 8, "Yeni")
        }, TripScheduleRelationshipConsts.Definitions.Select(value => (value.Id, value.Code, value.Name)));
        Assert.Equal(new (int, int, string)[]
        {
            (1, 0, "Planlı"), (2, 1, "Atlandı"), (3, 2, "Veri yok"), (4, 3, "Plansız")
        }, StopScheduleRelationshipConsts.Definitions.Select(value => (value.Id, value.Code, value.Name)));
        Assert.Equal(new (int, int, string)[]
        {
            (1, 0, "Durağa yaklaşıyor"), (2, 1, "Durakta"), (3, 2, "Sıradaki durağa gidiyor")
        }, VehicleStopStatusConsts.Definitions.Select(value => (value.Id, value.Code, value.Name)));
        Assert.Equal(new (int, int, string)[]
        {
            (1, 1, "Yapılmayan sefer"), (2, 2, "İptal edilen sefer"), (3, 3, "Durak atlama"),
            (4, 4, "Erken kalkış"), (5, 5, "Geç kalkış")
        }, ViolationTypeConsts.Definitions.Select(value => (value.Id, value.Code, value.Name)));
    }

    [Fact]
    public async Task ManagerAndMapper_CreateAndUpdateTripScheduleRelationship()
    {
        var repository = Substitute.For<IRealtimeLookupRepository<TripScheduleRelationship>>();
        var manager = new TripScheduleRelationshipManager(repository);
        var mapper = new TripScheduleRelationshipMapper();

        var entity = await manager.CreateAsync(mapper.Map(new CreateRealtimeLookupDto
        {
            Code = 25,
            Name = "  Yeni durum  "
        }));
        Assert.Equal((0, 25, "Yeni durum"), (entity.Id, entity.Code, entity.Name));
        Assert.Equal((0, 25, "Yeni durum"),
            (mapper.Map(entity).Id, mapper.Map(entity).Code, mapper.Map(entity).Name));

        await manager.UpdateAsync(entity, new RealtimeLookupUpdateModel { Code = 26, Name = "  Yeni ad  " });
        Assert.Equal((26, "Yeni ad"), (entity.Code, entity.Name));
    }

    [Fact]
    public async Task Migrations_CreateFourEmptyLookupTablesWithUniqueCodes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new GtfsRealtimeManagementDbContext(
            new DbContextOptionsBuilder<GtfsRealtimeManagementDbContext>().UseSqlite(connection).Options);

        await context.Database.MigrateAsync();
        Assert.Equal(context.Database.GetMigrations().Count(), (await context.Database.GetAppliedMigrationsAsync()).Count());
        Assert.False(await context.TripScheduleRelationships.AnyAsync());
        Assert.False(await context.StopScheduleRelationships.AnyAsync());
        Assert.False(await context.VehicleStopStatuses.AnyAsync());
        Assert.False(await context.ViolationTypes.AnyAsync());
        var entities = context.Model.GetEntityTypes()
            .Where(entity => typeof(RealtimeLookup).IsAssignableFrom(entity.ClrType)).ToList();
        Assert.Equal(4, entities.Count);
        Assert.All(entities, entity =>
        {
            Assert.Equal("gtfs_realtime_management", entity.GetSchema());
            Assert.Contains(entity.GetIndexes(), index => index.IsUnique &&
                index.Properties.Single().Name == nameof(RealtimeLookup.Code));
        });
    }
}
