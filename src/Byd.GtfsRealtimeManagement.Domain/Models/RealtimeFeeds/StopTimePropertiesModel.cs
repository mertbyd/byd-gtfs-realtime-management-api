namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT StopTimeUpdate.StopTimeProperties'in aynasidir.
// sistemdeki gorevi: Duragin gercek zamanli degisen ozelliklerini (atanan durak, tabela, binis/inis turu) tasir.
public class StopTimePropertiesModel
{
    public string? AssignedStopId { get; set; }
    public string? StopHeadsign { get; set; }

    // DropOffPickupType kodu: 0 REGULAR, 1 NONE, 2 PHONE_AGENCY, 3 COORDINATE_WITH_DRIVER.
    public int? PickupType { get; set; }
    public int? DropOffType { get; set; }
}
