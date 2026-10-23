namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT TripDescriptor.ModifiedTripSelector'in cevap aynasidir.
// sistemdeki gorevi: Degistirilmis seferin TripModifications kaydini ve asil seferini tasir.
public sealed class ModifiedTripSelectorDto
{
    // Foreign Keys
    public string? ModificationsId { get; set; }
    public string? AffectedTripId { get; set; }

    // Table Columns / Data
    public string? StartTime { get; set; }
    public string? StartDate { get; set; }
}
