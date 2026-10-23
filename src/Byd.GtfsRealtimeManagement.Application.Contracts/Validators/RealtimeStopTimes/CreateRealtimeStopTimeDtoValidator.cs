using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;

namespace Byd.GtfsRealtimeManagement.Validators.RealtimeStopTimes;

// islevi: Yeni durak satiri isteginin dogrulama tipini belirtir.
// sistemdeki gorevi: Ortak girdi kurallarini create sozlesmesine baglar.
public sealed class CreateRealtimeStopTimeDtoValidator : RealtimeStopTimeInputDtoValidatorBase<CreateRealtimeStopTimeDto>;
