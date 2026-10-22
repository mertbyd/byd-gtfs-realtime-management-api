using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: TripScheduleRelationship tablosunun kalici kimliklerini ve kodlarini tanimlar.
// sistemdeki gorevi: Seed akisi ve EF eslemesinin ayni varsayilan satirlari okumasini saglar.
public static class TripScheduleRelationshipConsts
{
    public const string TableName = "TripScheduleRelationships";
    public const string CodeIndex = "IX_TripScheduleRelationships_Code";

    public static readonly IReadOnlyList<(int Id, string Name, int Code)> Definitions =
    [
        (1, "Planlı", 0),
        (2, "Eklendi", 1),
        (3, "Plansız", 2),
        (4, "İptal", 3),
        (5, "Yerine konan", 5),
        (6, "Kopya", 6),
        (7, "Silindi", 7),
        (8, "Yeni", 8)
    ];
}
