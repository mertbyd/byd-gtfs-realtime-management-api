namespace Byd.GtfsRealtimeManagement.Entities.Lookups;

// islevi: TripScheduleRelationship lookup satirini tasir.
// sistemdeki gorevi: Realtime sefer kayitlarinin planlama durumuna kodla baglanmasini saglar.
public class TripScheduleRelationship : RealtimeLookup
{
    // EF materialization icin bos kabuk kurar.
    protected TripScheduleRelationship()
    {
    }

    // Kimligi, adi ve kodu verilen sefer planlama durumu satirini kurar.
    internal TripScheduleRelationship(int id, string name, int code) : base(id, name, code)
    {
    }
}
