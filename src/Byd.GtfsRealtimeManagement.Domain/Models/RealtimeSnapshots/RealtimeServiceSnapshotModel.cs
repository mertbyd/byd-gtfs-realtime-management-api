using System;
using System.Collections.Generic;

namespace Byd.GtfsRealtimeManagement.Models.RealtimeSnapshots;

// islevi: GTFS servisinin haftalik takvimini (calendar) ve tarih istisnalarini (calendar_dates) tasir.
// sistemdeki gorevi: Seferin hangi servis gununde calistigi bu takvimden bulunur.
public class RealtimeServiceSnapshotModel
{
    public string GtfsServiceId { get; set; } = default!;
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    public bool Sunday { get; set; }

    // Haftalik takvimin gecerlilik araligi; servisin haftalik takvimi yoksa bos.
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    // Tarih ile GTFS exception_type kodu; haftalik takvimin onune gecer.
    public Dictionary<DateOnly, int> Exceptions { get; set; } = [];
}
