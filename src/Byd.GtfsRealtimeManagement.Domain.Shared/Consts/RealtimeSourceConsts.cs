namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: RealtimeSource tablo ve tekil ad indeksi sozlesmesini tanimlar.
// sistemdeki gorevi: EF eslemesi ile is kurallarinin ayni adlari kullanmasini saglar.
public static class RealtimeSourceConsts
{
    public const string TableName = "RealtimeSources";
    public const string NameIndex = "IX_RealtimeSources_Scope_Name";
}
