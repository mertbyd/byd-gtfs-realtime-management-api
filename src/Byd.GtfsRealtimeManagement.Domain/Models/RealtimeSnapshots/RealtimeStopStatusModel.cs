using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: TripUpdates akisinin bir sefer orneginin bir duragi icin bildirdigi son durumu ve gecikmeyi tasir.
// sistemdeki gorevi: Gecikme her cekmede degistigi icin satira hemen yazilmaz; arac duraktan cikarken bu son deger yazilir.
public class RealtimeStopStatusModel
{
    public DateOnly ServiceDate { get; set; }
    public int TripScheduleRelationshipCode { get; set; }
    public int StopScheduleRelationshipCode { get; set; }

    // Kaynagin bildirdigi gecikme, saniye; duragin kendi bilgisi yoksa onceki duraktan devralinir.
    public int? ReportedDelay { get; set; }
    public string CorrelationId { get; set; } = default!;
}
