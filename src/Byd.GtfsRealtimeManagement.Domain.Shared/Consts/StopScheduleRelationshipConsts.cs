using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: StopScheduleRelationship tablosunun kalici kimliklerini ve kodlarini tanimlar.
// sistemdeki gorevi: Seed akisi ve EF eslemesinin ayni varsayilan satirlari okumasini saglar.
public static class StopScheduleRelationshipConsts
{
    public const string TableName = "StopScheduleRelationships";
    public const string CodeIndex = "IX_StopScheduleRelationships_Code";

    public static readonly IReadOnlyList<(int Id, string Name, int Code)> Definitions =
    [
        (1, "Planlı", 0),
        (2, "Atlandı", 1),
        (3, "Veri yok", 2),
        (4, "Plansız", 3)
    ];
}
