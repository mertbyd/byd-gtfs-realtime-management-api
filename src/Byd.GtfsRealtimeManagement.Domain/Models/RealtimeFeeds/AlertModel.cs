using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: GTFS-RT Alert'in aynasidir; gelmeyen istege bagli alan bos kalir.
// sistemdeki gorevi: Uyarinin gecerlilik araliklarini, kapsamini, nedenini, etkisini ve metinlerini tasir.
public class AlertModel
{
    // Standartta kullanimdan kalkti; yerine CommunicationPeriod ve ImpactPeriod.
    public List<TimeRangeModel> ActivePeriod { get; set; } = [];

    public List<TimeRangeModel> CommunicationPeriod { get; set; } = [];
    public List<TimeRangeModel> ImpactPeriod { get; set; } = [];

    // Zorunlu; uyarinin etkiledigi ajans, hat, sefer ve duraklar.
    public List<EntitySelectorModel> InformedEntity { get; set; } = [];

    // Cause kodu (1 UNKNOWN_CAUSE ... 13 SPECIAL_EVENT); gelmezse standarttaki varsayilan UNKNOWN_CAUSE.
    public int? Cause { get; set; }

    // Effect kodu (1 NO_SERVICE ... 11 ACCESSIBILITY_ISSUE); gelmezse standarttaki varsayilan UNKNOWN_EFFECT.
    public int? Effect { get; set; }

    public TranslatedStringModel? Url { get; set; }
    public TranslatedStringModel? HeaderText { get; set; }
    public TranslatedStringModel? DescriptionText { get; set; }
    public TranslatedStringModel? TtsHeaderText { get; set; }
    public TranslatedStringModel? TtsDescriptionText { get; set; }

    // SeverityLevel kodu: 1 UNKNOWN_SEVERITY, 2 INFO, 3 WARNING, 4 SEVERE; gelmezse UNKNOWN_SEVERITY.
    public int? SeverityLevel { get; set; }

    public TranslatedImageModel? Image { get; set; }
    public TranslatedStringModel? ImageAlternativeText { get; set; }
    public TranslatedStringModel? CauseDetail { get; set; }
    public TranslatedStringModel? EffectDetail { get; set; }
}
