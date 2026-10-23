using System;

namespace Byd.GtfsRealtimeManagement.Etos.RealtimeSources;

// Istekle ayni is biriminde aktif kaynak secimini baslatan yerel olay.
public class RealtimeSourceActivationRequestedEto
{
    public Guid TenantId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid RealtimeSourceId { get; set; }
}
