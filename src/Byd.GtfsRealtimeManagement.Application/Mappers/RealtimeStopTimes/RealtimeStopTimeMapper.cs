using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.Models.RealtimeStopTimes;
using Riok.Mapperly.Abstractions;

namespace Byd.GtfsRealtimeManagement.Mappers.RealtimeStopTimes;

// islevi: Durak satiri DTO, domain modeli ve entity donusumlerini uretir.
// sistemdeki gorevi: Application sinirinda manuel alan kopyalamayi onler.
[Mapper]
public partial class RealtimeStopTimeMapper
{
    public partial RealtimeStopTimeCreateModel Map(CreateRealtimeStopTimeDto input);
    public partial RealtimeStopTimeUpdateModel Map(UpdateRealtimeStopTimeDto input);
    public partial RealtimeStopTimeDto Map(RealtimeStopTime entity);
}
