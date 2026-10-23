using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Byd.GtfsRealtimeManagement.Services.RealtimeSources;

// islevi: Kaynak CRUD ve akis okuma API sozlesmesini tanimlar.
// sistemdeki gorevi: Controller ile uygulama katmanini ayirir.
public interface IRealtimeSourceAppService : IApplicationService
{
    Task<RealtimeSourceDto> CreateAsync(CreateRealtimeSourceDto input, CancellationToken cancellationToken = default);
    Task<RealtimeSourceDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RealtimeSourceDto> GetIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<RealtimeSourceDto>> GetFilteredListAsync(
        DataTableListInput<RealtimeSourceFilter> input, CancellationToken cancellationToken = default);
    Task<RealtimeSourceDto> UpdateAsync(
        Guid id, UpdateRealtimeSourceDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RealtimeSourceDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<VehiclePositionDto>> GetVehiclePositionsAsync(CancellationToken cancellationToken = default);
    Task<List<TripUpdateDto>> GetTripUpdatesAsync(CancellationToken cancellationToken = default);
    Task<List<AlertDto>> GetServiceAlertsAsync(CancellationToken cancellationToken = default);
    Task<FeedMessageDto> GetFeedMessageAsync(CancellationToken cancellationToken = default);
}
