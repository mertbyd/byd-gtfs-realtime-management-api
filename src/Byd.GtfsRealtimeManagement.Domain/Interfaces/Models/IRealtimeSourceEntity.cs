using System;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Volo.Abp.Domain.Entities;

namespace Byd.GtfsRealtimeManagement.Interfaces;

// islevi: Kaynak altindaki kalici satirlarin kaynak bagini tanimlar.
// sistemdeki gorevi: Aktif kaynak ve kapsam filtresinin bu satirlara ortak uygulanmasini saglar; tenant ve organizasyon
// kaynaktan belirlenir.
public interface IRealtimeSourceEntity : IEntity<Guid>
{
    // Satirin ait oldugu realtime kaynak kimligi.
    Guid RealtimeSourceId { get; }

    // Satirin bagli oldugu realtime kaynak navigasyonu.
    RealtimeSource RealtimeSource { get; }
}
