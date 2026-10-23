using System;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStopTimes;

// islevi: Durak satirinin duzenlenebilir alanlarini tasir.
// sistemdeki gorevi: Create ve update domain kurallarinin ortak girdisidir.
public abstract class RealtimeStopTimeInputModel
{
    public int TripScheduleRelationshipId { get; set; }
    public int StopScheduleRelationshipId { get; set; }
    public int? VehicleStopStatusId { get; set; }
    public int? ViolationTypeId { get; set; }
    public Guid GtfsFeedId { get; set; }
    public Guid? VehicleId { get; set; }
    public DateOnly ServiceDate { get; set; }
    public string? StartTime { get; set; }
    public string GtfsTripId { get; set; } = default!;
    public string GtfsRouteId { get; set; } = default!;
    public int StopSequence { get; set; }
    public string GtfsStopId { get; set; } = default!;
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? ReportedDelay { get; set; }
    public string? ArrivalCorrelationId { get; set; }
    public string? DepartureCorrelationId { get; set; }
    public string? StatusCorrelationId { get; set; }
}
