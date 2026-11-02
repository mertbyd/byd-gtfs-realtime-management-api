namespace Byd.GtfsRealtimeManagement.RealtimeFeeds;

// GTFS-RT akisinin son cekmedeki sagligi; degistiginde organizasyona bildirim gider, adi bildirimin basligidir. Tabloda
// tutulmaz.
public enum RealtimeFeedHealth
{
    // Akis cekildi, guncel ve plana uyuyor.
    Healthy = 0,

    // Kaynaga ulasilamadi ya da yanit zamaninda gelmedi.
    Unreachable = 1,

    // Yanit GTFS-RT olarak cozulemedi ya da desteklenmeyen surum/artimlilikta.
    Invalid = 2,

    // Baslik zamani bayatlik esiginden eski; akis islenmez.
    Stale = 3,

    // Akis organizasyonun aktif GTFS veri setine ait degil (feed_version farkli ya da hicbir atfi eslesmiyor); islenmez.
    PlanMismatch = 4
}
