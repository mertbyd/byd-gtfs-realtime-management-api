using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Volo.Abp.Application.Services;

namespace Byd.GtfsRealtimeManagement.Services.Lookups;

// islevi: Realtime lookup turlerinin olustur, oku, listele, guncelle ve sil API sozlesmesini tanimlar.
// sistemdeki gorevi: Her lookup turunun ayni CRUD sozlesmesini paylasmasini saglar.
public interface IRealtimeLookupCrudAppService : IApplicationService
{
    Task<RealtimeLookupDto> CreateAsync(CreateRealtimeLookupDto input, CancellationToken cancellationToken = default);
    Task<RealtimeLookupDto> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<List<RealtimeLookupDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<RealtimeLookupDto> UpdateAsync(int id, UpdateRealtimeLookupDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
