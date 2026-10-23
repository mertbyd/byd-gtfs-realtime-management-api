namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TripDescriptor.ModifiedTripSelector'in aynasidir.
// sistemdeki gorevi: Degistirilmis seferin hangi TripModifications kaydina ve hangi asil sefere bagli oldugunu tasir.
public class ModifiedTripSelectorModel
{
    public string? ModificationsId { get; set; }
    public string? AffectedTripId { get; set; }
    public string? StartTime { get; set; }
    public string? StartDate { get; set; }
}
