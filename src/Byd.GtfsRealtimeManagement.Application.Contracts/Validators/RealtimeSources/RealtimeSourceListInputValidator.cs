using FluentValidation;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Querying;
using Byd.GtfsRealtimeManagement.Dtos.RealtimeSources;

namespace Byd.GtfsRealtimeManagement.Validators.RealtimeSources;

// islevi: Kaynak liste isteginin sayfa sinirlarini dogrular.
// sistemdeki gorevi: DataTable sorgusuna sinirli isteklerin girmesini saglar.
public sealed class RealtimeSourceListInputValidator : AbstractValidator<DataTableListInput<RealtimeSourceFilter>>
{
    // Sayfa ve sayfa boyutu kurallarini kaydeder.
    public RealtimeSourceListInputValidator()
    {
        RuleFor(input => input.Page).GreaterThanOrEqualTo(DataTablePaging.FirstPage)
            .WithErrorCode(GeneralExceptionCodes.InvalidPage);
        RuleFor(input => input.PageSize).InclusiveBetween(1, DataTablePaging.MaxPageSize)
            .WithErrorCode(GeneralExceptionCodes.InvalidPageSize);
    }
}
