using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Models.Lookups;
using Riok.Mapperly.Abstractions;

namespace Byd.GtfsRealtimeManagement.Mappers.Lookups;

// islevi: ViolationType entity'si, domain modelleri ve public DTO'lar arasindaki eslemeleri uretir.
// sistemdeki gorevi: Application sinirinda manuel alan kopyalamayi onler.
[Mapper]
public partial class ViolationTypeMapper : IRealtimeLookupCrudMapper<ViolationType>
{
    public partial RealtimeLookupCreateModel Map(CreateRealtimeLookupDto input);
    public partial RealtimeLookupUpdateModel Map(UpdateRealtimeLookupDto input);
    public partial RealtimeLookupDto Map(ViolationType entity);
}
