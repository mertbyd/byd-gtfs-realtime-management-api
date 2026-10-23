namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: Arac ve plan kopyasinin ortak veritabanindan okundugu SQL metinlerini ve GTFS takvim istisna kodlarini tanimlar.
// sistemdeki gorevi: Arac modulunun ve GTFS modulunun sema/tablo adlarina bagli tek yerdir; o modullerde tablo adi
// degisirse yalniz bu dosya degisir.
public static class RealtimeSnapshotConsts
{
    // GTFS calendar_dates.exception_type: 1 servis o gun eklendi, 2 servis o gun kaldirildi.
    public const int ServiceAddedExceptionType = 1;
    public const int ServiceRemovedExceptionType = 2;

    public const string VehiclesSql = """
        select id as VehicleId, gtfs_vehicle_id as GtfsVehicleId
        from vehicle_management."Vehicles"
        where tenant_id = @tenantId and organization_id = @organizationId
        """;

    public const string ActiveFeedSql = """
        select id
        from gtfs_management."Feeds"
        where tenant_id = @tenantId and organization_id = @organizationId and is_active
        """;

    public const string TripsSql = """
        select t.gtfs_trip_id as GtfsTripId, sv.gtfs_service_id as GtfsServiceId, r.gtfs_route_id as GtfsRouteId,
               rt.code as RouteType, a.gtfs_agency_id as GtfsAgencyId
        from gtfs_management."Trips" t
        join gtfs_management."Services" sv on sv.id = t.service_id
        join gtfs_management."Routes" r on r.id = t.route_id
        join gtfs_management."RouteTypes" rt on rt.id = r.route_type_id
        left join gtfs_management."Agencies" a on a.id = r.agency_id
        where t.feed_id = @feedId
        """;

    public const string StopTimesSql = """
        select t.gtfs_trip_id as GtfsTripId, st.stop_sequence as StopSequence, s.gtfs_stop_id as GtfsStopId,
               st.arrival_time as ArrivalTime, st.departure_time as DepartureTime, s.latitude as Latitude,
               s.longitude as Longitude
        from gtfs_management."StopTimes" st
        join gtfs_management."Trips" t on t.id = st.trip_id
        left join gtfs_management."Stops" s on s.id = st.stop_id
        where st.feed_id = @feedId
        order by t.gtfs_trip_id, st.stop_sequence
        """;

    public const string StopIdsSql = """
        select gtfs_stop_id
        from gtfs_management."Stops"
        where feed_id = @feedId
        """;

    // Haftalik takvimi olmayan servis yalniz calendar_dates istisnalariyla calisir; gunleri kapali, tarih araligi bos gelir.
    public const string ServicesSql = """
        select sv.gtfs_service_id as GtfsServiceId,
               coalesce(c.monday, false) as Monday, coalesce(c.tuesday, false) as Tuesday,
               coalesce(c.wednesday, false) as Wednesday, coalesce(c.thursday, false) as Thursday,
               coalesce(c.friday, false) as Friday, coalesce(c.saturday, false) as Saturday,
               coalesce(c.sunday, false) as Sunday, c.start_date as StartDate, c.end_date as EndDate
        from gtfs_management."Services" sv
        left join gtfs_management."Calendars" c on c.service_id = sv.id
        where sv.feed_id = @feedId
        """;

    public const string CalendarDatesSql = """
        select sv.gtfs_service_id as GtfsServiceId, cd.date as Date, et.code as ExceptionType
        from gtfs_management."CalendarDates" cd
        join gtfs_management."Services" sv on sv.id = cd.service_id
        join gtfs_management."ExceptionTypes" et on et.id = cd.exception_type_id
        where cd.feed_id = @feedId
        """;
}
