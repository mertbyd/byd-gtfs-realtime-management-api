using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStreams;

// islevi: Bir akis turunun tek bildirimini tasir; GTFS-RT FeedHeader.incrementality gibi listenin tam mi yoksa yalniz
// degisenler mi oldugunu soyler.
// sistemdeki gorevi: Tam listede istemci elindekini bununla degistirir; degilse yalniz gelen kayitlari gunceller ve silindi
// isaretli olanlari atar.
public class RealtimeStreamModel<TEntity>
{
    public bool IsFullDataset { get; set; }
    public List<TEntity> Entities { get; set; } = [];
}
