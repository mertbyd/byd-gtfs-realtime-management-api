using System;
using Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStopTimes;

// islevi: Akistaki seferin plandaki karsiligini ve hangi servis gununun hangi ornegi oldugunu tasir.
// sistemdeki gorevi: Durak satiri bu ornegin servis gunu, baslangic saati ve sefer kimligiyle tekillesir.
public class RealtimeTripInstanceModel
{
    public RealtimeTripSnapshotModel Trip { get; set; } = default!;
    public DateOnly ServiceDate { get; set; }

    // GTFS-RT start_time; siklik tabanli ve kopya seferlerde ayni trip_id'nin orneklerini ayirir.
    public string? StartTime { get; set; }
}
