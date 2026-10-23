using FluentValidation;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;

namespace Byd.GtfsRealtimeManagement.Validators.RealtimeStopTimes;

// islevi: Durak satiri liste isteginin sayfa sinirlarini dogrular.
// sistemdeki gorevi: Core DataTable sorgusuna sinirli sayfalama uygular.
public sealed class RealtimeStopTimeListInputValidator : AbstractValidator<DataTableListInput<RealtimeStopTimeFilter>>
{
    // Sayfa numarasi ve boyutu araliklarini kaydeder.
    public RealtimeStopTimeListInputValidator()
    {
        RuleFor(input => input.Page).GreaterThanOrEqualTo(DataTablePaging.FirstPage)
            .WithErrorCode(GeneralExceptionCodes.InvalidPage);
        RuleFor(input => input.PageSize).InclusiveBetween(1, DataTablePaging.MaxPageSize)
            .WithErrorCode(GeneralExceptionCodes.InvalidPageSize);
    }
}
