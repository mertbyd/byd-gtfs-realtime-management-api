namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Planlanan seferin tek duragini; sirasini, durak kimligini, planlanan saatlerini ve konumunu tasir.
// sistemdeki gorevi: Plan kopyasinin Dapper ile yuklenen satiridir; GtfsTripId satiri seferine baglamak icindir.
public class RealtimeStopTimeSnapshotModel
{
    public string GtfsTripId { get; set; } = default!;
    public int StopSequence { get; set; }
    public string? GtfsStopId { get; set; }
    public string? ArrivalTime { get; set; }
    public string? DepartureTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
