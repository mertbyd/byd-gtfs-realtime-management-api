using System.Collections.Generic;
using Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeStreams;

// islevi: Gecerlilik araliginda ve kapsami planda karsiligi olan bir servis uyarisini tasir.
// sistemdeki gorevi: Her servis uyarisi cekmesinden sonra organizasyona giden canli akisin govdesine JSON olarak yazilir;
// istemci uyariyi kapsamindaki hat, durak ya da sefer ekraninda gosterir.
public class RealtimeServiceAlertStreamModel
{
    // FeedEntity.id; uyari cekmeler arasinda bu kimlikle izlenir.
    public string AlertId { get; set; } = default!;

    // Alert cause, effect ve severity_level kodlari; akis gondermediyse standarttaki varsayilan.
    public int Cause { get; set; }
    public int Effect { get; set; }
    public int SeverityLevel { get; set; }

    // Uyarinin gosterim araliklari (communication_period, yoksa active_period) ve hizmete etki araliklari, POSIX saniye.
    public List<TimeRangeModel> CommunicationPeriods { get; set; } = [];
    public List<TimeRangeModel> ImpactPeriods { get; set; } = [];

    // Kapsamdaki secicilerden planda karsiligi olanlar.
    public List<EntitySelectorModel> InformedEntities { get; set; } = [];

    // Uyarinin dillere gore metinleri.
    public TranslatedStringModel? HeaderText { get; set; }
    public TranslatedStringModel? DescriptionText { get; set; }
    public TranslatedStringModel? Url { get; set; }
    public TranslatedStringModel? CauseDetail { get; set; }
    public TranslatedStringModel? EffectDetail { get; set; }
}
