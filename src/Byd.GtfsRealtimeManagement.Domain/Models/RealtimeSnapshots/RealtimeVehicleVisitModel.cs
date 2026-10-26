using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Aracin seferindeki ilerleyisini cekmeler arasinda tasir: gectigi son durak, icinde bulundugu durak ve son konumu.
// sistemdeki gorevi: Giris ve cikis iki ayri cekmede bulundugu icin isleme adimi aracin onceki durumunu buradan okur.
public class RealtimeVehicleVisitModel
{
    // Aracin bulundugu sefer orneginin anahtari; sefer degisince ziyaret sifirlanir.
    public string TripKey { get; set; } = default!;

    // Kapanmis (gecilmis) son durak sirasi; henuz durak gecilmediyse -1.
    public int PassedSequence { get; set; } = -1;

    // Aracin yaricapinda bulundugu durak ve orada durup durmadigi.
    public int? CurrentSequence { get; set; }
    public bool Stopped { get; set; }

    // Akis hiz gondermezse hiz son iki konumdan hesaplanir.
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public DateTime? LastMeasuredAt { get; set; }
}
