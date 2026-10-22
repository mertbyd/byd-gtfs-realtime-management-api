namespace Byd.GtfsRealtimeManagement.Models.Lookups;

// islevi: Lookup olusturma ve guncelleme isteklerinin domain tarafindaki ortak alanlarini tasir.
// sistemdeki gorevi: Manager'in DTO'ya bagimli olmadan lookup kurallarini uygulamasini saglar.
public abstract class RealtimeLookupInputModel
{
    public int Code { get; set; }
    public string Name { get; set; } = default!;
}
