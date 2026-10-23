using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT VehiclePosition'in cevap aynasidir.
// sistemdeki gorevi: Aracin konumunu, hizini, duraga gore durumunu ve hizmet ettigi seferi tasir.
public sealed class VehiclePositionDto
{
    // Foreign Keys
    public string? StopId { get; set; }

    // Table Columns / Data
    public uint? CurrentStopSequence { get; set; }
    public int? CurrentStatus { get; set; }
    public ulong? Timestamp { get; set; }
    public int? CongestionLevel { get; set; }
    public int? OccupancyStatus { get; set; }
    public uint? OccupancyPercentage { get; set; }

    // Navigation Properties
    public TripDescriptorDto? Trip { get; set; }
    public VehicleDescriptorDto? Vehicle { get; set; }
    public PositionDto? Position { get; set; }
    public List<CarriageDetailsDto> MultiCarriageDetails { get; set; } = [];
}
