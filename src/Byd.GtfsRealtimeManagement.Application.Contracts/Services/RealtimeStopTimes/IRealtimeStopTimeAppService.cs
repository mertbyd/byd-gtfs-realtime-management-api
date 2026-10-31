using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Byd.GtfsRealtimeManagement.Services.RealtimeStopTimes;

// islevi: Durak satiri CRUD ve gecmis okuma sozlesmesini tanimlar.
// sistemdeki gorevi: Controller ile uygulama katmanini ayirir.
public interface IRealtimeStopTimeAppService : IApplicationService
{
    Task<RealtimeStopTimeDto> CreateAsync(CreateRealtimeStopTimeDto input, CancellationToken cancellationToken = default);
    Task<RealtimeStopTimeDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RealtimeStopTimeDto> GetForSourceAsync(
        Guid realtimeSourceId, Guid id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<RealtimeStopTimeDto>> GetFilteredListAsync(
        DataTableListInput<RealtimeStopTimeFilter> input, CancellationToken cancellationToken = default);
    Task<PagedResultDto<RealtimeStopTimeDto>> GetFilteredListForSourceAsync(
        Guid realtimeSourceId, DataTableListInput<RealtimeStopTimeFilter> input,
        CancellationToken cancellationToken = default);
    Task<RealtimeStopTimeDto> UpdateAsync(
        Guid id, UpdateRealtimeStopTimeDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
