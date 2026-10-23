using System;
using FluentValidation;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;
using Byd.GtfsRealtimeManagement.ExceptionCodes;

namespace Byd.GtfsRealtimeManagement.Validators.RealtimeStopTimes;

// islevi: Durak satiri girdisinin zorunlu alan ve araliklarini dogrular.
// sistemdeki gorevi: Create ve update isteklerine ortak sekil kurallari saglar.
public abstract class RealtimeStopTimeInputDtoValidatorBase<TDto> : AbstractValidator<TDto>
    where TDto : RealtimeStopTimeInputDto
{
    // Sefer, durak, gun, konum ve opsiyonel baslangic saati kurallarini kaydeder.
    protected RealtimeStopTimeInputDtoValidatorBase()
    {
        RuleFor(input => input.GtfsFeedId).NotEmpty().WithErrorCode(RealtimeStopTimeExceptionCodes.FeedRequired);
        RuleFor(input => input.TripScheduleRelationshipId).GreaterThan(0)
            .WithErrorCode(RealtimeStopTimeExceptionCodes.RelationshipRequired);
        RuleFor(input => input.StopScheduleRelationshipId).GreaterThan(0)
            .WithErrorCode(RealtimeStopTimeExceptionCodes.RelationshipRequired);
        RuleFor(input => input.VehicleStopStatusId).GreaterThan(0).When(input => input.VehicleStopStatusId.HasValue)
            .WithErrorCode(RealtimeStopTimeExceptionCodes.RelationshipRequired);
        RuleFor(input => input.ViolationTypeId).GreaterThan(0).When(input => input.ViolationTypeId.HasValue)
            .WithErrorCode(RealtimeStopTimeExceptionCodes.RelationshipRequired);
        RuleFor(input => input.ServiceDate).NotEqual(default(DateOnly))
            .WithErrorCode(RealtimeStopTimeExceptionCodes.ServiceDateRequired);
        RuleFor(input => input.GtfsTripId).NotEmpty().WithErrorCode(RealtimeStopTimeExceptionCodes.GtfsIdRequired);
        RuleFor(input => input.GtfsRouteId).NotEmpty().WithErrorCode(RealtimeStopTimeExceptionCodes.GtfsIdRequired);
        RuleFor(input => input.GtfsStopId).NotEmpty().WithErrorCode(RealtimeStopTimeExceptionCodes.GtfsIdRequired);
        RuleFor(input => input.StopSequence).GreaterThanOrEqualTo(0)
            .WithErrorCode(RealtimeStopTimeExceptionCodes.StopSequenceInvalid);
        RuleFor(input => input.Latitude).InclusiveBetween(-90d, 90d).When(input => input.Latitude.HasValue)
            .WithErrorCode(RealtimeStopTimeExceptionCodes.CoordinatesInvalid);
        RuleFor(input => input.Longitude).InclusiveBetween(-180d, 180d).When(input => input.Longitude.HasValue)
            .WithErrorCode(RealtimeStopTimeExceptionCodes.CoordinatesInvalid);
        RuleFor(input => input.StartTime).Matches("^[0-9]+:[0-5][0-9]:[0-5][0-9]$")
            .When(input => !string.IsNullOrWhiteSpace(input.StartTime))
            .WithErrorCode(RealtimeStopTimeExceptionCodes.StartTimeInvalid);
    }
}
