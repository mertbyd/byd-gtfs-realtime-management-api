using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;
using Volo.Abp.DependencyInjection;

namespace Byd.GtfsRealtimeManagement.Stores.RealtimeSnapshots;

// islevi: Organizasyon bazindaki arac ve plan kopyasini uygulama omru boyunca bellekte tutar.
// sistemdeki gorevi: Cekme iscisi ve olay handler'lari ayni kopyayi paylasir; uygulama yeniden baslayinca kopya
// organizasyonun ilk cekmesinde yeniden yuklenir.
public class RealtimeSnapshotStore : ISingletonDependency
{
    private readonly ConcurrentDictionary<Guid, RealtimeOrganizationSnapshotModel> snapshots = new();

    public virtual RealtimeOrganizationSnapshotModel? Find(Guid organizationId)
        => snapshots.GetValueOrDefault(organizationId);

    // Ayni anda iki yukleme olursa ilk eklenen kopya kalir.
    public virtual RealtimeOrganizationSnapshotModel Add(Guid organizationId, RealtimeOrganizationSnapshotModel snapshot)
        => snapshots.GetOrAdd(organizationId, snapshot);
}
