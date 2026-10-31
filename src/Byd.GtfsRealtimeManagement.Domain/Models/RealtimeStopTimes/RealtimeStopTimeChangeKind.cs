namespace Byd.GtfsRealtimeManagement.Models.RealtimeStopTimes;

// islevi: Durak satirina yazilacak degisikligin turunu tanimlar.
// sistemdeki gorevi: Isleme adiminin her degisikligi satira hangi alanlarla yazacagini secer.
public enum RealtimeStopTimeChangeKind
{
    // Arac durak yaricapinda ve duruyor: giris zamani ve konumu.
    Arrival = 1,

    // Duran arac duraktan ayrildi: cikis zamani.
    Departure = 2,

    // Arac durmadan gecti ya da durak gorulmeden geride kaldi: yalniz gecisi kanitlayan correlation.
    PassedThrough = 3,

    // Sefer iptal/silindi ya da durak atlandi: planlama durumu.
    Status = 4
}
