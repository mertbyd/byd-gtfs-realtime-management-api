namespace Byd.GtfsRealtimeManagement.Consts;

// islevi: Durak olcumunde izinli dinamik filtre alanlarini tanimlar.
// sistemdeki gorevi: DataTable alan adlarini kararli sozlesmede toplar.
public static class RealtimeStopTimeFilterFields
{
    public const string ServiceDate = nameof(ServiceDate);
    public const string GtfsTripId = nameof(GtfsTripId);
    public const string GtfsRouteId = nameof(GtfsRouteId);
    public const string VehicleId = nameof(VehicleId);
    public const string ViolationTypeId = nameof(ViolationTypeId);

    public static class PropertyPaths
    {
        public const string ServiceDate = RealtimeStopTimeFilterFields.ServiceDate;
        public const string GtfsTripId = RealtimeStopTimeFilterFields.GtfsTripId;
        public const string GtfsRouteId = RealtimeStopTimeFilterFields.GtfsRouteId;
        public const string VehicleId = RealtimeStopTimeFilterFields.VehicleId;
        public const string ViolationTypeId = RealtimeStopTimeFilterFields.ViolationTypeId;
    }
}
