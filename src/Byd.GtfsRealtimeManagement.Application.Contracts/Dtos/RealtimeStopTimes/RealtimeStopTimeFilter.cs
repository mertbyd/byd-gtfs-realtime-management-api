using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Interfaces;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;

// islevi: Durak olcumu listesinin filtrelenebilir alanlarini tasir.
// sistemdeki gorevi: Core DataTable sorgusunu tipli alanlarla sinirlar.
public sealed class RealtimeStopTimeFilter : IRealtimeSourceFilter
{
    [DataTable(RealtimeStopTimeFilterFields.RealtimeSourceId, RealtimeStopTimeFilterFields.PropertyPaths.RealtimeSourceId)]
    public DataTableFilterItem? RealtimeSourceId { get; set; }

    [DataTable(RealtimeStopTimeFilterFields.ServiceDate, RealtimeStopTimeFilterFields.PropertyPaths.ServiceDate)]
    public DataTableFilterItem? ServiceDate { get; set; }

    [DataTable(RealtimeStopTimeFilterFields.GtfsTripId, RealtimeStopTimeFilterFields.PropertyPaths.GtfsTripId)]
    public DataTableFilterItem? GtfsTripId { get; set; }

    [DataTable(RealtimeStopTimeFilterFields.GtfsRouteId, RealtimeStopTimeFilterFields.PropertyPaths.GtfsRouteId)]
    public DataTableFilterItem? GtfsRouteId { get; set; }

    [DataTable(RealtimeStopTimeFilterFields.VehicleId, RealtimeStopTimeFilterFields.PropertyPaths.VehicleId)]
    public DataTableFilterItem? VehicleId { get; set; }

    [DataTable(RealtimeStopTimeFilterFields.ViolationTypeId, RealtimeStopTimeFilterFields.PropertyPaths.ViolationTypeId)]
    public DataTableFilterItem? ViolationTypeId { get; set; }
}
