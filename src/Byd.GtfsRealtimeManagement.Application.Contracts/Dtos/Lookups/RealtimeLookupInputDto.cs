namespace Byd.GtfsRealtimeManagement.Dtos.Lookups;

// islevi: Realtime lookup satirinin istemciden gelen duzenlenebilir alanlarini tasir.
// sistemdeki gorevi: Olusturma ve guncelleme isteklerinin ortak alan sozlesmesidir.
public abstract class RealtimeLookupInputDto
{
    // Table Columns / Data
    public int Code { get; set; }
    public string Name { get; set; } = default!;
}
