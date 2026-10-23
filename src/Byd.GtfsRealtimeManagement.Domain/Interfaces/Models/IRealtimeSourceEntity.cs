using System;
using Volo.Abp.Domain.Entities;

namespace Byd.GtfsRealtimeManagement.Interfaces;

// islevi: Kaynak altindaki kalici satirlarin FK alanini tanimlar.
// sistemdeki gorevi: Aktif kaynak ve kapsam filtresinin bu satirlara ortak uygulanmasini saglar.
public interface IRealtimeSourceEntity : IEntity<Guid>
{
    Guid RealtimeSourceId { get; }
}
