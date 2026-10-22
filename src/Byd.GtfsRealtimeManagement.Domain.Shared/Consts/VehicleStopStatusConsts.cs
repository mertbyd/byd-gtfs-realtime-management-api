using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: VehicleStopStatus tablosunun kalici kimliklerini ve kodlarini tanimlar.
// sistemdeki gorevi: Seed akisi ve EF eslemesinin ayni varsayilan satirlari okumasini saglar.
public static class VehicleStopStatusConsts
{
    public const string TableName = "VehicleStopStatuses";
    public const string CodeIndex = "IX_VehicleStopStatuses_Code";

    public static readonly IReadOnlyList<(int Id, string Name, int Code)> Definitions =
    [
        (1, "Durağa yaklaşıyor", 0),
        (2, "Durakta", 1),
        (3, "Sıradaki durağa gidiyor", 2)
    ];
}
