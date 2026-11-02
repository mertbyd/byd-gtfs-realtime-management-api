using System.Collections.Generic;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStreams;

// islevi: Bir akis turunun tek bildirimini tasir; GTFS-RT FeedHeader.incrementality gibi listenin tam mi yoksa yalniz
// degisenler mi oldugunu, akisin sagligini ve plana eslesmeyen kayit sayisini soyler.
// sistemdeki gorevi: Tam listede istemci elindekini bununla degistirir; degilse yalniz gelen kayitlari gunceller ve silindi
// isaretli olanlari atar. Saglik degisimi cekme adiminda organizasyona bildirim olarak gider.
public class RealtimeStreamModel<TEntity>
{
    public bool IsFullDataset { get; set; }
    public List<TEntity> Entities { get; set; } = [];

    // Akisin bu cekmedeki sagligi; saglikli degilse liste bos ve kismidir.
    public RealtimeFeedHealth Health { get; set; }

    // Bu cekmede plana eslesmedigi ya da eksik oldugu icin islenmeyen kayit sayisi.
    public int UnmatchedCount { get; set; }
}
