using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Byd.GtfsRealtimeManagement.Services.Lookups;
using Volo.Abp;
using Xunit;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore.Lookups;

public class RealtimeLookupCrudTests : GtfsRealtimeManagementEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task TripScheduleRelationshipService_CreatesReadsUpdatesAndDeletes()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var service = ServiceProvider.GetRequiredService<ITripScheduleRelationshipAppService>();
            var created = await service.CreateAsync(new CreateRealtimeLookupDto { Code = 25, Name = "Yeni durum" });
            Assert.True(created.Id > 0);
            await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(
                new CreateRealtimeLookupDto { Code = 25, Name = "Tekrar" }));
            Assert.Equal("Yeni durum", (await service.GetAsync(created.Id)).Name);
            Assert.Contains(await service.GetListAsync(), item => item.Id == created.Id);

            var updated = await service.UpdateAsync(created.Id,
                new UpdateRealtimeLookupDto { Code = 26, Name = "Yeni ad" });
            Assert.Equal((26, "Yeni ad"), (updated.Code, updated.Name));

            await service.DeleteAsync(created.Id);
            Assert.DoesNotContain(await service.GetListAsync(), item => item.Id == created.Id);
        });
    }
}
