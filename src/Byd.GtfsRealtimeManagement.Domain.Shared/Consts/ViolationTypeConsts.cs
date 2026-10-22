using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: ViolationType tablosunun kalici kimliklerini ve kodlarini tanimlar.
// sistemdeki gorevi: Seed akisi ve EF eslemesinin ayni varsayilan satirlari okumasini saglar.
public static class ViolationTypeConsts
{
    public const string TableName = "ViolationTypes";
    public const string CodeIndex = "IX_ViolationTypes_Code";

    public static readonly IReadOnlyList<(int Id, string Name, int Code)> Definitions =
    [
        (1, "Yapılmayan sefer", 1),
        (2, "İptal edilen sefer", 2),
        (3, "Durak atlama", 3),
        (4, "Erken kalkış", 4),
        (5, "Geç kalkış", 5)
    ];
}
