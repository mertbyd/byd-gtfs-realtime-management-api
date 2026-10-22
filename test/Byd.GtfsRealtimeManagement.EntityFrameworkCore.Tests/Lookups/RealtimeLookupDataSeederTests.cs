using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Data.Lookups;
using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Byd.GtfsRealtimeManagement.Services.Lookups;
using Volo.Abp.Data;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.Lookups;

public class RealtimeLookupDataSeederTests : GtfsRealtimeManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Seeder_AddsDefaultsOnceAndKeepsUserChanges()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var service = ServiceProvider.GetRequiredService<ITripScheduleRelationshipAppService>();
            var seeder = ServiceProvider.GetRequiredService<TripScheduleRelationshipDataSeeder>();
            await seeder.SeedAsync(new DataSeedContext());

            var relationships = await service.GetListAsync();
            Assert.Equal(TripScheduleRelationshipConsts.Definitions.Count, relationships.Count);
            var cancelled = relationships.Single(value => value.Code == 3);
            Assert.Equal("İptal", cancelled.Name);

            await service.UpdateAsync(cancelled.Id, new UpdateRealtimeLookupDto { Code = 3, Name = "İptal edildi" });
            await seeder.SeedAsync(new DataSeedContext());

            var reseeded = await service.GetListAsync();
            Assert.Equal(TripScheduleRelationshipConsts.Definitions.Count, reseeded.Count);
            Assert.Equal("İptal edildi", reseeded.Single(value => value.Code == 3).Name);
        });
    }
}
