using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.Models.RealtimeSources;
using Riok.Mapperly.Abstractions;

namespace Byd.GtfsRealtimeManagement.Mappers.RealtimeSources;

// islevi: Kaynak DTO, domain modeli ve entity arasindaki eslemeleri uretir.
// sistemdeki gorevi: Application sinirinda manuel alan kopyalamayi onler.
[Mapper]
public partial class RealtimeSourceMapper
{
    public partial RealtimeSourceCreateModel Map(CreateRealtimeSourceDto input);
    public partial RealtimeSourceUpdateModel Map(UpdateRealtimeSourceDto input);
    public partial RealtimeSourceDto Map(RealtimeSource entity);
}
