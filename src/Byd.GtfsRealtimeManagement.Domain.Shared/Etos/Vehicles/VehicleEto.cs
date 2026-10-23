using System;
using Byd.GtfsRealtimeManagement.Consts;
using Volo.Abp.EventBus;

namespace Byd.GtfsRealtimeManagement.Etos.Vehicles;

// islevi: Arac modulunun entity olayinda tasidigi arac kimligidir; arac modulundeki VehicleEto ile ayni adi ve alanlari
// tasir.
// sistemdeki gorevi: Realtime arac kopyasini arac eklenince, guncellenince ve silinince gunceller.
[EventName(RealtimeExternalEventNames.Vehicle)]
public class VehicleEto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid OrganizationId { get; set; }
    public string? GtfsAgencyId { get; set; }
    public string GtfsVehicleId { get; set; } = default!;
}
