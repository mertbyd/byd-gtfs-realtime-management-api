namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT FeedEntity'nin aynasidir; sefer guncellemesi, arac konumu ya da uyaridan en az biri doludur.
// sistemdeki gorevi: Akistaki tek varligi tasir. Standarttaki deneysel shape, stop ve trip_modifications varliklari
// modele alinmaz.
public class FeedEntityModel
{
    // Zorunlu; akis icinde tekil varlik kimligi.
    public string Id { get; set; } = default!;

    // Yalniz DIFFERENTIAL akista anlamlidir.
    public bool? IsDeleted { get; set; }

    public TripUpdateModel? TripUpdate { get; set; }
    public VehiclePositionModel? Vehicle { get; set; }
    public AlertModel? Alert { get; set; }
}
