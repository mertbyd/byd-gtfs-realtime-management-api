using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;

namespace Byd.GtfsRealtimeManagement.Interfaces.RealtimeStopTimes;

// islevi: Islenmis durak satiri icin detayli okuma, saklama silmesi ve core repository sozlesmesini tanimlar.
// sistemdeki gorevi: Domain katmanini EF sorgu uygulamasindan ayirir.
public interface IRealtimeStopTimeRepository : IExtendedRepository<RealtimeStopTime>
{
    Task<RealtimeStopTime?> FindWithLookupsAsync(Guid id, CancellationToken cancellationToken);

    Task<List<RealtimeStopTime>> GetListForTripsAsync(Guid realtimeSourceId, DateOnly serviceDate,
        IReadOnlyCollection<string> gtfsTripIds, CancellationToken cancellationToken = default);

    // Butun kaynaklarda servis gunu verilen gunden once olan satirlari siler; silinen satir sayisini dondurur.
    Task<int> DeleteBeforeAsync(DateOnly serviceDate, CancellationToken cancellationToken = default);
}
