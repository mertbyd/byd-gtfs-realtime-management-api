namespace Byd.GtfsRealtimeManagement.Entities.Lookups;

// islevi: ViolationType lookup satirini tasir.
// sistemdeki gorevi: Realtime durak kayitlarinin ihlal turune kodla baglanmasini saglar.
public class ViolationType : RealtimeLookup
{
    // EF materialization icin bos kabuk kurar.
    protected ViolationType()
    {
    }

    // Kimligi, adi ve kodu verilen ihlal turu satirini kurar.
    internal ViolationType(int id, string name, int code) : base(id, name, code)
    {
    }
}
