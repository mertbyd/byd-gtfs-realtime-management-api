using System;
using Byd.GtfsRealtimeManagement.Consts;
using Volo.Abp.EventBus;

namespace Byd.GtfsRealtimeManagement.Etos.Vehicles;

// islevi: Akista gorulen ve kayitli olmayan aracin GTFS-RT VehicleDescriptor bilgisini arac modulune tasir.
// sistemdeki gorevi: Arac modulundeki VehicleRegistrationRequestedEto ile ayni adi ve alanlari tasir; arac modulu araci
// kendi kapsaminda realtime kaynakli olarak kaydeder.
[EventName(RealtimeExternalEventNames.VehicleRegistrationRequested)]
public class VehicleRegistrationRequestedEto
{
    public Guid TenantId { get; set; }
    public Guid OrganizationId { get; set; }
    public string GtfsVehicleId { get; set; } = default!;
    public string? GtfsAgencyId { get; set; }
    public string? Label { get; set; }
    public string? RegistrationNumber { get; set; }
    public int? RouteType { get; set; }
    public int? WheelchairAccessible { get; set; }
}
