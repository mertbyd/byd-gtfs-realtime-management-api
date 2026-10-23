using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;

namespace Byd.GtfsRealtimeManagement.Interfaces.RealtimeStopTimes;

// islevi: Islenmis durak satiri icin detayli okuma ve core repository sozlesmesini tanimlar.
// sistemdeki gorevi: Domain katmanini EF sorgu uygulamasindan ayirir.
public interface IRealtimeStopTimeRepository : IExtendedRepository<RealtimeStopTime>
{
    Task<RealtimeStopTime?> FindWithLookupsAsync(Guid id, CancellationToken cancellationToken);
}
