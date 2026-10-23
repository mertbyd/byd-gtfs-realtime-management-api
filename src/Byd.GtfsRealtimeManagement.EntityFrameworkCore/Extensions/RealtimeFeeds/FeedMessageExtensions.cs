using System.Linq;
using Google.Protobuf;
using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.ExceptionCodes;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;
using TransitRealtime;
using Volo.Abp;

namespace Byd.GtfsRealtimeManagement.Extensions.RealtimeFeeds;

// islevi: GTFS-RT protobuf yanitini cozer, basligini dogrular ve FeedMessage modeline cevirir.
// sistemdeki gorevi: Uretilen protobuf siniflarinin okuyucudan disari cikmamasini saglar. Gelmeyen istege bagli alan modelde
// bos kalir; "gelmedi" ile "0 geldi" protobuf'un Has* alanlariyla ayrilir, bu yuzden esleme Mapperly ile degil elle yazilir.
internal static class FeedMessageExtensions
{
    // Baytlari cozer, basligi dogrular ve modele cevirir.
    public static FeedMessageModel ToFeedMessageModel(this byte[] content)
    {
        var message = Parse(content);
        EnsureSupported(message.Header);
        return new FeedMessageModel
        {
            Header = message.Header.ToModel(),
            Entity = message.Entity.Select(entity => entity.ToModel()).ToList()
        };
    }

    // Protobuf olarak cozulemeyen yanit kodlu hataya cevrilir; ham yanit log modulunde kalir.
    private static FeedMessage Parse(byte[] content)
    {
        try
        {
            return FeedMessage.Parser.ParseFrom(content);
        }
        catch (InvalidProtocolBufferException exception)
        {
            throw new BusinessException(RealtimeFeedExceptionCodes.FeedInvalid, innerException: exception);
        }
    }

    // Standartta tanimsiz surum ve DIFFERENTIAL akis islenmez.
    private static void EnsureSupported(FeedHeader header)
    {
        if (!RealtimeFeedConsts.SupportedVersions.Contains(header.GtfsRealtimeVersion)
            || header.Incrementality == FeedHeader.Types.Incrementality.Differential)
            throw new BusinessException(RealtimeFeedExceptionCodes.FeedNotSupported);
    }

    private static FeedHeaderModel ToModel(this FeedHeader header) => new()
    {
        GtfsRealtimeVersion = header.GtfsRealtimeVersion,
        Incrementality = header.HasIncrementality ? (int)header.Incrementality : null,
        Timestamp = header.HasTimestamp ? header.Timestamp : null,
        FeedVersion = header.HasFeedVersion ? header.FeedVersion : null
    };

    private static FeedEntityModel ToModel(this FeedEntity entity) => new()
    {
        Id = entity.Id,
        IsDeleted = entity.HasIsDeleted ? entity.IsDeleted : null,
        TripUpdate = entity.TripUpdate?.ToModel(),
        Vehicle = entity.Vehicle?.ToModel(),
        Alert = entity.Alert?.ToModel()
    };

    private static TripUpdateModel ToModel(this TripUpdate tripUpdate) => new()
    {
        Trip = tripUpdate.Trip.ToModel(),
        Vehicle = tripUpdate.Vehicle?.ToModel(),
        StopTimeUpdate = tripUpdate.StopTimeUpdate.Select(update => update.ToModel()).ToList(),
        Timestamp = tripUpdate.HasTimestamp ? tripUpdate.Timestamp : null,
        Delay = tripUpdate.HasDelay ? tripUpdate.Delay : null,
        TripProperties = tripUpdate.TripProperties?.ToModel()
    };

    private static StopTimeUpdateModel ToModel(this TripUpdate.Types.StopTimeUpdate update) => new()
    {
        StopSequence = update.HasStopSequence ? update.StopSequence : null,
        StopId = update.HasStopId ? update.StopId : null,
        Arrival = update.Arrival?.ToModel(),
        Departure = update.Departure?.ToModel(),
        DepartureOccupancyStatus = update.HasDepartureOccupancyStatus ? (int)update.DepartureOccupancyStatus : null,
        ScheduleRelationship = update.HasScheduleRelationship ? (int)update.ScheduleRelationship : null,
        StopTimeProperties = update.StopTimeProperties?.ToModel()
    };

    private static StopTimeEventModel ToModel(this TripUpdate.Types.StopTimeEvent stopTimeEvent) => new()
    {
        Delay = stopTimeEvent.HasDelay ? stopTimeEvent.Delay : null,
        Time = stopTimeEvent.HasTime ? stopTimeEvent.Time : null,
        Uncertainty = stopTimeEvent.HasUncertainty ? stopTimeEvent.Uncertainty : null,
        ScheduledTime = stopTimeEvent.HasScheduledTime ? stopTimeEvent.ScheduledTime : null
    };

    private static StopTimePropertiesModel ToModel(
        this TripUpdate.Types.StopTimeUpdate.Types.StopTimeProperties properties) => new()
    {
        AssignedStopId = properties.HasAssignedStopId ? properties.AssignedStopId : null,
        StopHeadsign = properties.HasStopHeadsign ? properties.StopHeadsign : null,
        PickupType = properties.HasPickupType ? (int)properties.PickupType : null,
        DropOffType = properties.HasDropOffType ? (int)properties.DropOffType : null
    };

    private static TripPropertiesModel ToModel(this TripUpdate.Types.TripProperties properties) => new()
    {
        TripId = properties.HasTripId ? properties.TripId : null,
        StartDate = properties.HasStartDate ? properties.StartDate : null,
        StartTime = properties.HasStartTime ? properties.StartTime : null,
        ShapeId = properties.HasShapeId ? properties.ShapeId : null,
        TripHeadsign = properties.HasTripHeadsign ? properties.TripHeadsign : null,
        TripShortName = properties.HasTripShortName ? properties.TripShortName : null
    };

    private static VehiclePositionModel ToModel(this VehiclePosition position) => new()
    {
        Trip = position.Trip?.ToModel(),
        Vehicle = position.Vehicle?.ToModel(),
        Position = position.Position?.ToModel(),
        CurrentStopSequence = position.HasCurrentStopSequence ? position.CurrentStopSequence : null,
        StopId = position.HasStopId ? position.StopId : null,
        CurrentStatus = position.HasCurrentStatus ? (int)position.CurrentStatus : null,
        Timestamp = position.HasTimestamp ? position.Timestamp : null,
        CongestionLevel = position.HasCongestionLevel ? (int)position.CongestionLevel : null,
        OccupancyStatus = position.HasOccupancyStatus ? (int)position.OccupancyStatus : null,
        OccupancyPercentage = position.HasOccupancyPercentage ? position.OccupancyPercentage : null,
        MultiCarriageDetails = position.MultiCarriageDetails.Select(carriage => carriage.ToModel()).ToList()
    };

    private static CarriageDetailsModel ToModel(this VehiclePosition.Types.CarriageDetails carriage) => new()
    {
        Id = carriage.HasId ? carriage.Id : null,
        Label = carriage.HasLabel ? carriage.Label : null,
        OccupancyStatus = carriage.HasOccupancyStatus ? (int)carriage.OccupancyStatus : null,
        OccupancyPercentage = carriage.HasOccupancyPercentage ? carriage.OccupancyPercentage : null,
        CarriageSequence = carriage.HasCarriageSequence ? carriage.CarriageSequence : null
    };

    private static PositionModel ToModel(this Position position) => new()
    {
        Latitude = position.Latitude,
        Longitude = position.Longitude,
        Bearing = position.HasBearing ? position.Bearing : null,
        Odometer = position.HasOdometer ? position.Odometer : null,
        Speed = position.HasSpeed ? position.Speed : null
    };

    private static TripDescriptorModel ToModel(this TripDescriptor trip) => new()
    {
        TripId = trip.HasTripId ? trip.TripId : null,
        RouteId = trip.HasRouteId ? trip.RouteId : null,
        DirectionId = trip.HasDirectionId ? trip.DirectionId : null,
        StartTime = trip.HasStartTime ? trip.StartTime : null,
        StartDate = trip.HasStartDate ? trip.StartDate : null,
        ScheduleRelationship = trip.HasScheduleRelationship ? (int)trip.ScheduleRelationship : null,
        ModifiedTrip = trip.ModifiedTrip?.ToModel()
    };

    private static ModifiedTripSelectorModel ToModel(this TripDescriptor.Types.ModifiedTripSelector selector) => new()
    {
        ModificationsId = selector.HasModificationsId ? selector.ModificationsId : null,
        AffectedTripId = selector.HasAffectedTripId ? selector.AffectedTripId : null,
        StartTime = selector.HasStartTime ? selector.StartTime : null,
        StartDate = selector.HasStartDate ? selector.StartDate : null
    };

    private static VehicleDescriptorModel ToModel(this VehicleDescriptor vehicle) => new()
    {
        Id = vehicle.HasId ? vehicle.Id : null,
        Label = vehicle.HasLabel ? vehicle.Label : null,
        LicensePlate = vehicle.HasLicensePlate ? vehicle.LicensePlate : null,
        WheelchairAccessible = vehicle.HasWheelchairAccessible ? (int)vehicle.WheelchairAccessible : null
    };

    private static AlertModel ToModel(this Alert alert) => new()
    {
#pragma warning disable CS0612 // active_period standartta kullanimdan kalkti ama kaynaklar hala gonderiyor (Vilnius dahil).
        ActivePeriod = alert.ActivePeriod.Select(range => range.ToModel()).ToList(),
#pragma warning restore CS0612
        CommunicationPeriod = alert.CommunicationPeriod.Select(range => range.ToModel()).ToList(),
        ImpactPeriod = alert.ImpactPeriod.Select(range => range.ToModel()).ToList(),
        InformedEntity = alert.InformedEntity.Select(selector => selector.ToModel()).ToList(),
        Cause = alert.HasCause ? (int)alert.Cause : null,
        Effect = alert.HasEffect ? (int)alert.Effect : null,
        Url = alert.Url?.ToModel(),
        HeaderText = alert.HeaderText?.ToModel(),
        DescriptionText = alert.DescriptionText?.ToModel(),
        TtsHeaderText = alert.TtsHeaderText?.ToModel(),
        TtsDescriptionText = alert.TtsDescriptionText?.ToModel(),
        SeverityLevel = alert.HasSeverityLevel ? (int)alert.SeverityLevel : null,
        Image = alert.Image?.ToModel(),
        ImageAlternativeText = alert.ImageAlternativeText?.ToModel(),
        CauseDetail = alert.CauseDetail?.ToModel(),
        EffectDetail = alert.EffectDetail?.ToModel()
    };

    private static TimeRangeModel ToModel(this TimeRange range) => new()
    {
        Start = range.HasStart ? range.Start : null,
        End = range.HasEnd ? range.End : null
    };

    private static EntitySelectorModel ToModel(this EntitySelector selector) => new()
    {
        AgencyId = selector.HasAgencyId ? selector.AgencyId : null,
        RouteId = selector.HasRouteId ? selector.RouteId : null,
        RouteType = selector.HasRouteType ? selector.RouteType : null,
        Trip = selector.Trip?.ToModel(),
        StopId = selector.HasStopId ? selector.StopId : null,
        DirectionId = selector.HasDirectionId ? selector.DirectionId : null
    };

    private static TranslatedStringModel ToModel(this TranslatedString text) => new()
    {
        Translation = text.Translation
            .Select(translation => new TranslationModel
            {
                Text = translation.Text,
                Language = translation.HasLanguage ? translation.Language : null
            })
            .ToList()
    };

    private static TranslatedImageModel ToModel(this TranslatedImage image) => new()
    {
        LocalizedImage = image.LocalizedImage
            .Select(localized => new LocalizedImageModel
            {
                Url = localized.Url,
                MediaType = localized.MediaType,
                Language = localized.HasLanguage ? localized.Language : null
            })
            .ToList()
    };
}
