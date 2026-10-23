using System;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;

// islevi: Durak olcumunu lookup adlari ve kanit kimlikleriyle dondurur.
// sistemdeki gorevi: Kalici entity'yi public cevap sozlesmesinden ayirir.
public sealed class RealtimeStopTimeDto
{
    // Primary Key
    public Guid Id { get; set; }

    // Foreign Keys
    public Guid RealtimeSourceId { get; set; }
    public int TripScheduleRelationshipId { get; set; }
    public int StopScheduleRelationshipId { get; set; }
    public int? VehicleStopStatusId { get; set; }
    public int? ViolationTypeId { get; set; }
    // GTFS modulundeki kayda yonelik harici korelasyon kimligi; ad bu modulde tutulmaz.
    public Guid GtfsFeedId { get; set; }
    // Arac modulundeki kayda yonelik harici korelasyon kimligi; ad bu modulde tutulmaz.
    public Guid? VehicleId { get; set; }

    // Table Columns / Data
    public string RealtimeSourceName { get; set; } = default!;
    public string TripScheduleRelationshipName { get; set; } = default!;
    public string StopScheduleRelationshipName { get; set; } = default!;
    public string? VehicleStopStatusName { get; set; }
    public string? ViolationTypeName { get; set; }
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
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
