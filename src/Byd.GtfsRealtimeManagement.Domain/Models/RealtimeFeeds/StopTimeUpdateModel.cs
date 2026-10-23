namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT TripUpdate.StopTimeUpdate'in aynasidir; durak StopSequence ya da StopId ile belirtilir.
// sistemdeki gorevi: Bir durak icin varis/kalkis zamanini ve durak durumunu tasir.
public class StopTimeUpdateModel
{
    public uint? StopSequence { get; set; }
    public string? StopId { get; set; }
    public StopTimeEventModel? Arrival { get; set; }
    public StopTimeEventModel? Departure { get; set; }

    // VehiclePosition.OccupancyStatus kodu; duraktan kalkistan sonraki beklenen doluluk.
    public int? DepartureOccupancyStatus { get; set; }

    // StopTimeUpdate.ScheduleRelationship kodu (0 SCHEDULED, 1 SKIPPED, 2 NO_DATA, 3 UNSCHEDULED); StopScheduleRelationship
    // lookup'inin Code'u. Gelmezse standarttaki varsayilan SCHEDULED.
    public int? ScheduleRelationship { get; set; }

    public StopTimePropertiesModel? StopTimeProperties { get; set; }
}
