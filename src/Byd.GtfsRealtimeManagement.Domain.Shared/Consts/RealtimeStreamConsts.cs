namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: Islenmis realtime verinin canli akisla gonderilme kurallarini tanimlar.
// sistemdeki gorevi: Akis adi akis turunun adidir (VehiclePositions, TripUpdates); istemci log hub'indan gelen streamReceived
// mesajini bu adla ayirt eder, liste govdede JSON olarak gelir.
public static class RealtimeStreamConsts
{
    // Sefer tahminlerinin tam liste olarak yeniden gonderilme araligi (saniye); aradaki cekmelerde yalniz degisenler gider,
    // sonradan baglanan istemci butun seferleri en gec bu surede alir.
    public const int TripUpdatesFullDatasetSeconds = 60;
}
