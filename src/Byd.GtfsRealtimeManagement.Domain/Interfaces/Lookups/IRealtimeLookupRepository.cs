using Byd.CoreExtensions.Interfaces;
using Byd.GtfsRealtimeManagement.Entities.Lookups;

namespace Byd.GtfsRealtimeManagement.Interfaces.Lookups;

// islevi: Realtime lookup turleri icin core repository sozlesmesini tek generic sozlesmede bildirir.
// sistemdeki gorevi: Lookup Manager'larinin silme oncesi iliski kontrolunu core'un IsDataUsedAsync yetenegiyle yapmasini
// saglar.
public interface IRealtimeLookupRepository<TEntity> : IExtendedRepository<TEntity>
    where TEntity : RealtimeLookup
{
}
