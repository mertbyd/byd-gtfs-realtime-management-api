using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Consts;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;

// islevi: Kaynak listeleme icin izinli filtre alanlarini tanimlar.
// sistemdeki gorevi: Core DataTable sorgusunu tipli alanlarla sinirlar.
public sealed class RealtimeSourceFilter
{
    [DataTable(RealtimeSourceFilterFields.Name, RealtimeSourceFilterFields.PropertyPaths.Name)]
    public DataTableFilterItem? Name { get; set; }

    [DataTable(RealtimeSourceFilterFields.IsActive, RealtimeSourceFilterFields.PropertyPaths.IsActive)]
    public DataTableFilterItem? IsActive { get; set; }

    [DataTable(RealtimeSourceFilterFields.CreationTime, RealtimeSourceFilterFields.PropertyPaths.CreationTime)]
    public DataTableFilterItem? CreationTime { get; set; }
}
