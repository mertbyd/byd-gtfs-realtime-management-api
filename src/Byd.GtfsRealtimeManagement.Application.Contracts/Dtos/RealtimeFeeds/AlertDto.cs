using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Dtos.RealtimeFeeds;

// islevi: GTFS-RT Alert'in cevap aynasidir.
// sistemdeki gorevi: Uyarinin araliklarini, kapsamini, nedenini, etkisini ve metinlerini tasir.
public sealed class AlertDto
{
    // Table Columns / Data
    public int? Cause { get; set; }
    public int? Effect { get; set; }
    public int? SeverityLevel { get; set; }

    // Navigation Properties
    public List<TimeRangeDto> ActivePeriod { get; set; } = [];
    public List<TimeRangeDto> CommunicationPeriod { get; set; } = [];
    public List<TimeRangeDto> ImpactPeriod { get; set; } = [];
    public List<EntitySelectorDto> InformedEntity { get; set; } = [];
    public TranslatedStringDto? Url { get; set; }
    public TranslatedStringDto? HeaderText { get; set; }
    public TranslatedStringDto? DescriptionText { get; set; }
    public TranslatedStringDto? TtsHeaderText { get; set; }
    public TranslatedStringDto? TtsDescriptionText { get; set; }
    public TranslatedImageDto? Image { get; set; }
    public TranslatedStringDto? ImageAlternativeText { get; set; }
    public TranslatedStringDto? CauseDetail { get; set; }
    public TranslatedStringDto? EffectDetail { get; set; }
}
