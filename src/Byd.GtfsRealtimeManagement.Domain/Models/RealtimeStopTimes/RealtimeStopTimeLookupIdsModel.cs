using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStopTimes;

// islevi: GTFS-RT durum kodlarinin lookup tablolarindaki kimliklerini tasir.
// sistemdeki gorevi: Isleme adimi bir is biriminde yazdigi satirlarin lookup baglarini bu eslemeden kurar.
public class RealtimeStopTimeLookupIdsModel
{
    public Dictionary<int, int> TripScheduleRelationships { get; set; } = [];
    public Dictionary<int, int> StopScheduleRelationships { get; set; } = [];
    public Dictionary<int, int> VehicleStopStatuses { get; set; } = [];
    public Dictionary<int, int> ViolationTypes { get; set; } = [];
}
