using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Servisin tek tarihteki istisnasini (calendar_dates) tasir.
// sistemdeki gorevi: Takvim kopyasinin Dapper ile yuklenen satiridir; servisin istisna sozlugune islenir.
public class RealtimeCalendarDateSnapshotModel
{
    public string GtfsServiceId { get; set; } = default!;
    public DateOnly Date { get; set; }
    public int ExceptionType { get; set; }
}
