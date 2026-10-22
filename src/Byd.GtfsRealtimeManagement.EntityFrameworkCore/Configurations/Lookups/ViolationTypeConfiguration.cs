using Byd.GtfsRealtimeManagement.Consts;
using Byd.GtfsRealtimeManagement.Entities.Lookups;

namespace Byd.GtfsRealtimeManagement.EntityFrameworkCore;

// islevi: ViolationType tablosunu ortak lookup EF kurallariyla esler.
// sistemdeki gorevi: Tablo ve kod indeksini modulun semasina baglar.
public sealed class ViolationTypeConfiguration()
    : RealtimeLookupConfiguration<ViolationType>(ViolationTypeConsts.TableName, ViolationTypeConsts.CodeIndex);
