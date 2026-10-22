using System;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;

// islevi: Kaynagin adreslerini, durumunu ve audit zamanlarini tasir.
// sistemdeki gorevi: Entity'yi public cevap sozlesmesinden ayirir.
public sealed class RealtimeSourceDto
{
    // Primary Key
    public Guid Id { get; set; }

    // Table Columns / Data
    public string Name { get; set; } = default!;
    public string? VehiclePositionsUrl { get; set; }
    public string? TripUpdatesUrl { get; set; }
    public string? ServiceAlertsUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
