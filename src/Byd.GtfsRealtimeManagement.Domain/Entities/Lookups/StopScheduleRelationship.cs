namespace Byd.GtfsRealtimeManagement.Entities.Lookups;

// islevi: StopScheduleRelationship lookup satirini tasir.
// sistemdeki gorevi: Realtime durak kayitlarinin planlama durumuna kodla baglanmasini saglar.
public class StopScheduleRelationship : RealtimeLookup
{
    // EF materialization icin bos kabuk kurar.
    protected StopScheduleRelationship()
    {
    }

    // Kimligi, adi ve kodu verilen durak planlama durumu satirini kurar.
    internal StopScheduleRelationship(int id, string name, int code) : base(id, name, code)
    {
    }
}
