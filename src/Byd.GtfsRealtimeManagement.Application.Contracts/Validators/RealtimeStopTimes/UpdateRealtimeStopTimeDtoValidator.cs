using Byd.GtfsRealtimeManagement.Dtos.RealtimeStopTimes;

namespace Byd.GtfsRealtimeManagement.Validators.RealtimeStopTimes;

// islevi: Durak satiri guncelleme isteginin dogrulama tipini belirtir.
// sistemdeki gorevi: Ortak girdi kurallarini update sozlesmesine baglar.
public sealed class UpdateRealtimeStopTimeDtoValidator : RealtimeStopTimeInputDtoValidatorBase<UpdateRealtimeStopTimeDto>;
