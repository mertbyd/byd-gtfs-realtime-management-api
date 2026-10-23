using System.Collections.Generic;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using Riok.Mapperly.Abstractions;

namespace Byd.GtfsRealtimeManagement.Mappers.RealtimeFeeds;

// islevi: GTFS-RT FeedMessage modelini ve akislarin kendi mesaj listelerini ic ice butun alanlariyla cevap
// DTO'suna esler.
// sistemdeki gorevi: Application sinirinda manuel alan kopyalamayi onler.
[Mapper]
public partial class RealtimeFeedMapper
{
    public partial FeedMessageDto Map(FeedMessageModel model);
    public partial List<VehiclePositionDto> Map(List<VehiclePositionModel> models);
    public partial List<TripUpdateDto> Map(List<TripUpdateModel> models);
    public partial List<AlertDto> Map(List<AlertModel> models);
}
