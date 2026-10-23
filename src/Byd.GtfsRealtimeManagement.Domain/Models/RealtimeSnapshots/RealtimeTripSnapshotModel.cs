using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Aktif feed'deki planlanan seferi; servisini, hattini, hattin GTFS route_type kodunu, isletmecisini ve duraklarini
// tasir.
// sistemdeki gorevi: Akistaki trip_id'nin planda karsiligini ve aracin kip/isletmeci bilgisini verir.
public class RealtimeTripSnapshotModel
{
    public string GtfsTripId { get; set; } = default!;
    public string GtfsServiceId { get; set; } = default!;
    public string GtfsRouteId { get; set; } = default!;
    public int RouteType { get; set; }
    public string? GtfsAgencyId { get; set; }
    public List<RealtimeStopTimeSnapshotModel> StopTimes { get; set; } = [];
}
