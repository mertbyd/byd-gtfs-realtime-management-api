using System;
using Byd.GtfsRealtimeManagement.Consts;
using Volo.Abp.EventBus;

namespace Byd.GtfsRealtimeManagement.Etos.GtfsFeeds;

// islevi: GTFS modulunun aktif feed'i ya da aktif feed'in icerigi degistiginde yayinladigi olaydir; GTFS modulundeki
// FeedChangedEto ile ayni adi ve alanlari tasir.
// sistemdeki gorevi: Realtime plan kopyasinin gecikmesiz yenilenmesini tetikler.
[EventName(RealtimeExternalEventNames.GtfsFeedChanged)]
public class GtfsFeedChangedEto
{
    public Guid TenantId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid FeedId { get; set; }
}
