namespace Byd.GtfsRealtimeManagement.Dtos.Lookups;

// islevi: Realtime lookup satirinin kimligini, kodunu ve adini tasir.
// sistemdeki gorevi: Persistence entity'sini public cevap sozlesmesinden ayirir.
public class RealtimeLookupDto
{
    // Primary Key
    public int Id { get; set; }

    // Table Columns / Data
    public int Code { get; set; }
    public string Name { get; set; } = default!;
}
