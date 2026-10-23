using System;
using Byd.GtfsRealtimeManagement.Entities.Lookups;
using Byd.GtfsRealtimeManagement.Entities.RealtimeSources;
using Byd.GtfsRealtimeManagement.Interfaces;
using Volo.Abp.Domain.Entities.Auditing;

namespace Byd.GtfsRealtimeManagement.Entities.RealtimeStopTimes;

// islevi: Bir sefer orneginin tek duragindaki islenmis realtime olcumleri tasir.
// sistemdeki gorevi: Kaynak, servis gunu ve durak sirasiyla tekillestirilen kalici satirdir.
public class RealtimeStopTime : AuditedAggregateRoot<Guid>, IRealtimeSourceEntity
{
    // Satiri ureten kaynagin kimligi; kapsam bu kaynaktan gelir.
    public Guid RealtimeSourceId { get; internal set; }

    // Seferin GTFS-RT planlama durumu.
    public int TripScheduleRelationshipId { get; internal set; }

    // Duragin GTFS-RT planlama durumu.
    public int StopScheduleRelationshipId { get; internal set; }

    // Aracin duraga iliskin bildirdigi istege bagli durum.
    public int? VehicleStopStatusId { get; internal set; }

    // Bu durak satirindan uretilen istege bagli ihlal turu.
    public int? ViolationTypeId { get; internal set; }

    // Eslestirmede kullanilan harici statik GTFS feed kimligi.
    public Guid GtfsFeedId { get; internal set; }

    // Arac modulundeki istege bagli harici arac kimligi.
    public Guid? VehicleId { get; internal set; }

    // Seferin servis gunu.
    public DateOnly ServiceDate { get; internal set; }

    // Gun icindeki sefer ornegini ayiran GTFS baslangic saati.
    public string? StartTime { get; internal set; }

    // Harici statik GTFS sefer metin kimligi.
    public string GtfsTripId { get; internal set; } = default!;

    // Harici statik GTFS hat metin kimligi.
    public string GtfsRouteId { get; internal set; } = default!;

    // Sefer icindeki durak sirasi.
    public int StopSequence { get; internal set; }

    // Harici statik GTFS durak metin kimligi.
    public string GtfsStopId { get; internal set; } = default!;

    // Gerceklesen duraga giris zamani, UTC.
    public DateTime? ArrivalTime { get; internal set; }

    // Gerceklesen duraktan cikis zamani, UTC.
    public DateTime? DepartureTime { get; internal set; }

    // Giriste olculen WGS-84 enlem.
    public double? Latitude { get; internal set; }

    // Giriste olculen WGS-84 boylam.
    public double? Longitude { get; internal set; }

    // Kaynagin bildirdigi gecikme, saniye.
    public int? ReportedDelay { get; internal set; }

    // Girisi kanitlayan dis servis log correlation kimligi.
    public string? ArrivalCorrelationId { get; internal set; }

    // Cikisi veya duragin gecilmesini kanitlayan correlation kimligi.
    public string? DepartureCorrelationId { get; internal set; }

    // Son durum ve gecikmeyi kanitlayan correlation kimligi.
    public string? StatusCorrelationId { get; internal set; }

    // Kaynagin navigasyonu.
    public RealtimeSource RealtimeSource { get; internal set; } = default!;

    // Sefer durumunun navigasyonu.
    public TripScheduleRelationship TripScheduleRelationship { get; internal set; } = default!;

    // Durak durumunun navigasyonu.
    public StopScheduleRelationship StopScheduleRelationship { get; internal set; } = default!;

    // Arac durak durumunun navigasyonu.
    public VehicleStopStatus? VehicleStopStatus { get; internal set; }

    // Ihlal turunun navigasyonu.
    public ViolationType? ViolationType { get; internal set; }

    // EF materialization icin bos kabuk kurar.
    protected RealtimeStopTime()
    {
    }

    // Manager'in dogruladigi aktif kaynaga satir kurar.
    internal RealtimeStopTime(Guid id, Guid realtimeSourceId) : base(id)
    {
        RealtimeSourceId = realtimeSourceId;
    }
}
