namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: Realtime'in dinledigi ve yayinladigi, sahibi baska modul olan dagitik olaylarin tel adlarini tanimlar.
// sistemdeki gorevi: Arac ve GTFS modullerinin olay adlariyla birebir aynidir; olaylar pakete degil ada gore eslesir.
public static class RealtimeExternalEventNames
{
    // ABP arac entity olaylarinin koku; olay adlari .Created, .Updated ve .Deleted ekiyle olusur.
    public const string Vehicle = "VehicleManagement.Vehicle";
    public const string VehicleRegistrationRequested = "VehicleManagement.Vehicles.RegistrationRequested";
    public const string GtfsFeedChanged = "GtfsManagement.Feeds.Changed";
}
