namespace Byd.GtfsRealtimeManagement.Entities.Lookups;

// islevi: VehicleStopStatus lookup satirini tasir.
// sistemdeki gorevi: Realtime durak kayitlarinin arac durak durumuna kodla baglanmasini saglar.
public class VehicleStopStatus : RealtimeLookup
{
    // EF materialization icin bos kabuk kurar.
    protected VehicleStopStatus()
    {
    }

    // Kimligi, adi ve kodu verilen arac durak durumu satirini kurar.
    internal VehicleStopStatus(int id, string name, int code) : base(id, name, code)
    {
    }
}
