using System;
using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: Organizasyonun aktif GTFS feed'inin surumunu, seferlerini, hat, isletmeci ve durak kimliklerini ve servis
// takvimlerini tasir.
// sistemdeki gorevi: Akistaki trip_id, route_id ve stop_id'nin planda karsiligi, akisin ayni veri setine ait olup olmadigi
// ve seferin hangi gun calistigi bu kopyadan okunur.
public class RealtimePlanSnapshotModel
{
    public Guid FeedId { get; set; }

    // feed_info.feed_version; veri seti surum vermediyse bos.
    public string? FeedVersion { get; set; }

    public Dictionary<string, RealtimeTripSnapshotModel> Trips { get; set; } = [];
    public HashSet<string> StopIds { get; set; } = [];

    // Seferlerin hatlari ve isletmecileri; uyari kapsaminin planda karsiligi bunlardan okunur.
    public HashSet<string> RouteIds { get; set; } = [];
    public HashSet<string> AgencyIds { get; set; } = [];
    public Dictionary<string, RealtimeServiceSnapshotModel> Services { get; set; } = [];
}
