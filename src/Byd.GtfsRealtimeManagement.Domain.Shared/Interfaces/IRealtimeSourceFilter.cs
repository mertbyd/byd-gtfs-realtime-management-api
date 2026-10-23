using Byd.CoreExtensions.Querying;

namespace Byd.GtfsRealtimeManagement.Interfaces;

// islevi: Kaynak altindaki satir listelerinin kaynak kapsamli DataTable filtre alanini bildirir.
// sistemdeki gorevi: Domain kapsam kuralini transport DTO'suna baglamadan ortak filtre sozlesmesiyle yurutur.
public interface IRealtimeSourceFilter
{
    DataTableFilterItem? RealtimeSourceId { get; set; }
}
