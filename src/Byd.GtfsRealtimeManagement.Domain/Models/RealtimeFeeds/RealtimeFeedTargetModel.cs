using System;
using Byd.GtfsRealtimeManagement.RealtimeFeeds;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeFeeds;

// islevi: Bir kaynagin cekilecek tek akisini, kapsamini ve aralik ayarinin adini tasir.
// sistemdeki gorevi: Cekme iscisi akis turune ozel kod yazmadan bu hedefleri dolasir.
public class RealtimeFeedTargetModel
{
    public Guid SourceId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OrganizationId { get; set; }
    public string SourceName { get; set; } = default!;
    public RealtimeFeedType FeedType { get; set; }
    public string Url { get; set; } = default!;
    public string IntervalSettingName { get; set; } = default!;
}
