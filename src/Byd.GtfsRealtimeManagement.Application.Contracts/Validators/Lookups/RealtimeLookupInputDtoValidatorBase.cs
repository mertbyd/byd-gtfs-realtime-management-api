using FluentValidation;
using Byd.GtfsRealtimeManagement.Dtos.Lookups;
using Byd.GtfsRealtimeManagement.ExceptionCodes;

namespace Byd.GtfsRealtimeManagement.Validators.Lookups;

// islevi: Lookup olusturma ve guncelleme isteklerinin ortak alan kurallarini tanimlar.
// sistemdeki gorevi: Ad zorunlulugu ve kod araligini tek dogrulayici tabaninda toplar.
public abstract class RealtimeLookupInputDtoValidatorBase<TDto> : AbstractValidator<TDto>
    where TDto : RealtimeLookupInputDto
{
    protected RealtimeLookupInputDtoValidatorBase()
    {
        RuleFor(input => input.Name)
            .NotEmpty().WithErrorCode(RealtimeLookupExceptionCodes.NameRequired);
        RuleFor(input => input.Code)
            .GreaterThanOrEqualTo(0).WithErrorCode(RealtimeLookupExceptionCodes.CodeInvalid);
    }
}
