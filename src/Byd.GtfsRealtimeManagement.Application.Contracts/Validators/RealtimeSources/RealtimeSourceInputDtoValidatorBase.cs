using System;
using FluentValidation;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;
using Byd.GtfsRealtimeManagement.ExceptionCodes;

namespace Byd.GtfsRealtimeManagement.Validators.RealtimeSources;

// islevi: Kaynak adi ve adreslerinin sekil kurallarini dogrular.
// sistemdeki gorevi: Create ve update icin ayni istek dogrulamasini kullanir.
public abstract class RealtimeSourceInputDtoValidatorBase<TDto> : AbstractValidator<TDto>
    where TDto : RealtimeSourceInputDto
{
    // Zorunlu ad, en az bir adres ve HTTP(S) adres bicimini kaydeder.
    protected RealtimeSourceInputDtoValidatorBase()
    {
        RuleFor(input => input.Name).NotEmpty().WithErrorCode(RealtimeSourceExceptionCodes.NameRequired);
        RuleFor(input => input)
            .Must(input => !string.IsNullOrWhiteSpace(input.VehiclePositionsUrl)
                           || !string.IsNullOrWhiteSpace(input.TripUpdatesUrl)
                           || !string.IsNullOrWhiteSpace(input.ServiceAlertsUrl))
            .WithErrorCode(RealtimeSourceExceptionCodes.UrlRequired);
        RuleFor(input => input.VehiclePositionsUrl).Must(IsHttpUrl)
            .WithErrorCode(RealtimeSourceExceptionCodes.UrlInvalid);
        RuleFor(input => input.TripUpdatesUrl).Must(IsHttpUrl)
            .WithErrorCode(RealtimeSourceExceptionCodes.UrlInvalid);
        RuleFor(input => input.ServiceAlertsUrl).Must(IsHttpUrl)
            .WithErrorCode(RealtimeSourceExceptionCodes.UrlInvalid);
    }

    // Bos istege bagli adresi kabul eder; dolu adresin mutlak HTTP(S) olmasini ister.
    private static bool IsHttpUrl(string? value)
        => string.IsNullOrWhiteSpace(value)
           || (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
}
