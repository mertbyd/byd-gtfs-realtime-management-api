using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;

namespace Byd.GtfsRealtimeManagement.Interfaces.RealtimeSources;

// islevi: RealtimeSource icin core repository sozlesmesini bildirir.
// sistemdeki gorevi: Domain kurallarini EF uygulamasindan ayirir.
public interface IRealtimeSourceRepository : IExtendedRepository<RealtimeSource>
{
}
